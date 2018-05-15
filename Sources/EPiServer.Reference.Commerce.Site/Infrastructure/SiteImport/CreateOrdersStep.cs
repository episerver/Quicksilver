using EPiServer.Commerce.Internal.Migration.Steps;
using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.Reference.Commerce.Site.Features.Cart.Services;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Infrastructure.SiteImport.Templates;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.SiteImport
{
    [ServiceConfiguration(typeof(IMigrationStep))]
    public class CreateOrdersStep : IMigrationStep
    {
        private IProgressMessenger _progressMessenger;
        private Injected<ICartService> _cartService = default(Injected<ICartService>);
        private Injected<IOrderGroupCalculator> _orderGroupCalculator = default(Injected<IOrderGroupCalculator>);
        private Injected<IOrderGroupFactory> _orderGroupFactory = default(Injected<IOrderGroupFactory>);
        private Injected<IOrderRepository> _orderRepository = default(Injected<IOrderRepository>);

        public int Order => 1100;

        public string Name => "Quicksilver create customers and orders";

        public string Description => "Creates a sample set of customers and orders.";

        public bool Execute(IProgressMessenger progressMessenger)
        {
            _progressMessenger = progressMessenger;

            try
            {
                _progressMessenger.AddProgressMessageText("Creating customers and orders...", false, 0);

                CreateContactsAndOrders();

                return true;
            }
            catch (Exception ex)
            {
                _progressMessenger.AddProgressMessageText($"CreateCustomers failed: {ex.Message} Stack trace: {ex.StackTrace}", true, 0);
                LogManager.GetLogger(GetType()).Error(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Reads customers and their orders from an external file and creates them in the current site.
        /// </summary>
        private void CreateContactsAndOrders()
        {
            foreach (var customer in GetCustomersToImport())
            {
                var contact = CustomerContact.CreateInstance();

                contact.UserId = customer.Email;
                contact.Email = customer.Email;
                contact.FirstName = customer.FirstName;
                contact.LastName = customer.LastName;
                contact.FullName = $"{contact.FirstName} {contact.LastName}";
                contact.RegistrationSource = "Imported customer";

                contact.SaveChanges();

                MapAddressesFromCustomerToContact(customer, contact);

                contact.SaveChanges();

                foreach (var cart in customer.Carts)
                {
                    var order = CreateOrder(contact, cart);
                    _orderRepository.Service.Save(order);
                }

                foreach (var purchaseOrder in customer.PurchaseOrders)
                {
                    var order = CreateOrder(contact, purchaseOrder);
                    ProcessAndSaveAsPurchaseOrder(contact, order);
                }
            }
        }

        /// <summary>
        /// Adds a payment, shipping address, billing address and finally saves the order as a purchase order.
        /// </summary>
        /// <param name="contact">The order customer.</param>
        /// <param name="cart">The cart to be processed and saved.</param>
        private void ProcessAndSaveAsPurchaseOrder(CustomerContact contact, ICart cart)
        {
            var payment = CreatePayment(contact, cart);
            var contactAddress = contact.ContactAddresses.First();
            var orderAddress = CreateOrderAddress(cart, contactAddress);
            cart.GetFirstShipment().ShippingAddress = orderAddress;
            cart.AddPayment(payment);
            payment.BillingAddress = orderAddress;
            payment.Status = PaymentStatus.Processed.ToString();

            _orderRepository.Service.SaveAsPurchaseOrder(cart);
        }

        /// <summary>
        /// Creates a new credit card payment associated with <paramref name="contact"/>.
        /// </summary>
        /// <param name="contact">The owner contact of the credit card.</param>
        /// <param name="cart">The <see cref="ICart"/> the payment is for.</param>
        /// <returns>A new generic credit card payment.</returns>
        private IPayment CreatePayment(CustomerContact contact, ICart cart)
        {
            var paymentMethod = new GenericCreditCardPaymentMethod();
            var payment = paymentMethod.CreatePayment(cart.GetTotal(_orderGroupCalculator.Service), cart);
            payment.CustomerName = contact.FullName;

            return payment;
        }

        /// <summary>
        /// Creates a new <see cref="IOrderAddress"/> based on <paramref name="contactAddress"/>.
        /// </summary>
        /// <param name="orderGroup">The order group used for creating the new address.</param>
        /// <param name="contactAddress">The address from which all information is gathered.</param>
        /// <returns>An instance of a new <see cref="IOrderAddress"/>.</returns>
        private IOrderAddress CreateOrderAddress(IOrderGroup orderGroup, CustomerAddress contactAddress)
        {
            var orderAddress = orderGroup.CreateOrderAddress(_orderGroupFactory.Service, contactAddress.Name);

            orderAddress.Id = contactAddress.Name;
            orderAddress.City = contactAddress.City;
            orderAddress.CountryCode = contactAddress.CountryCode;
            orderAddress.CountryName = contactAddress.CountryName;
            orderAddress.DaytimePhoneNumber = contactAddress.DaytimePhoneNumber;
            orderAddress.Email = contactAddress.Email;
            orderAddress.EveningPhoneNumber = contactAddress.EveningPhoneNumber;
            orderAddress.FirstName = contactAddress.FirstName;
            orderAddress.LastName = contactAddress.LastName;
            orderAddress.Line1 = contactAddress.Line1;
            orderAddress.Line2 = contactAddress.Line2;
            orderAddress.Organization = contactAddress.Organization;
            orderAddress.PostalCode = contactAddress.PostalCode;
            orderAddress.RegionCode = contactAddress.RegionCode;
            orderAddress.RegionName = contactAddress.RegionName;

            return orderAddress;
        }

        /// <summary>
        /// Creates an order and assigns it to the provided customer.
        /// </summary>
        /// <param name="customer">The customer the new order belongs to.</param>
        /// <param name="orderGroup">The import template for the order group.</param>
        /// <returns>An <see cref="ICart"/> with line items as stated in the order group template.</returns>
        private ICart CreateOrder(CustomerContact customer, OrderGroupTemplate orderGroup)
        {
            var order = _cartService.Service.LoadOrCreateCart(_cartService.Service.DefaultCartName);
            order.CustomerId = customer.PrimaryKeyId.Value;

            foreach (var item in orderGroup.Details)
            {
                _cartService.Service.AddToCart(order, item.SKU, item.Quantity);
            }

            _cartService.Service.ValidateCart(order);

            return order;
        }

        /// <summary>
        /// Creates a <see cref="CustomerAddress"/> for each existing address in <paramref name="customer"/> and adds it to <paramref name="contact"/>.
        /// </summary>
        /// <param name="customer">The <see cref="CustomerTemplate"/> holding the details about the addresses to be created.</param>
        /// <param name="contact">The <see cref="CustomerContact"/> that will be given any created address.</param>
        private static void MapAddressesFromCustomerToContact(CustomerTemplate customer, CustomerContact contact)
        {
            foreach (var importedAddress in customer.Addresses)
            {
                var address = CustomerAddress.CreateInstance();

                address.Name = "Default address";
                address.PrimaryKeyId = new PrimaryKeyId(importedAddress.AddressId);
                address.City = importedAddress.City;
                address.CountryCode = importedAddress.CountryCode;
                address.CountryName = importedAddress.CountryName;
                address.FirstName = customer.FirstName;
                address.LastName = customer.LastName;
                address.Line1 = importedAddress.Line1;
                address.RegionCode = importedAddress.RegionCode;
                address.RegionName = importedAddress.RegionName;
                address.State = importedAddress.State;
                address.AddressType = CustomerAddressTypeEnum.Public | CustomerAddressTypeEnum.Shipping | CustomerAddressTypeEnum.Billing;

                contact.AddContactAddress(address);
            }
        }

        /// <summary>
        /// Reads an external file and deserializes its content to a collection of <see cref="CustomerTemplate"/>s.
        /// </summary>
        /// <returns>A collection of <see cref="CustomerTemplate"/>s.</returns>
        private IEnumerable<CustomerTemplate> GetCustomersToImport()
        {
            string filePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, @"App_Data\Orders.json");
            string json;

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                json = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<IEnumerable<CustomerTemplate>>(json);
        }
    }
}
using EPiServer.Commerce.Initialization;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using EPiServer.Commerce.Orders.Cloud.Client.Providers;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    [ModuleDependency(typeof(CartModeInitializationModule))]
    [InitializableModule]
    public class CloudStorageInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton(new OrderClient<CloudyOrder>(new StorageSettings()));
            context.Services.AddSingleton(new CartClient<CloudyCart>(new StorageSettings()));
            context.Services.AddSingleton(new PaymentPlanClient<CloudyPaymentPlan>());

            context.Services.RemoveAll<IPurchaseOrderProvider>();
            context.Services.AddSingleton<IPurchaseOrderProvider, CloudPurchaseOrderProvider>();

            context.Services.RemoveAll<IPurchaseOrderRepository>();
            context.Services.AddSingleton<IPurchaseOrderRepository, CloudPurchaseOrderProvider>();

            context.Services.RemoveAll<IPaymentPlanProvider>();
            context.Services.AddSingleton<IPaymentPlanProvider, CloudPaymentPlanProvider>();

            context.Services.RemoveAll<ICartProvider>();
            context.Services.AddSingleton<ICartProvider>(locator => locator.GetInstance<CloudCartProvider>());

            context.Services.RemoveAll<IPurchaseOrderFactory>();
            context.Services.AddSingleton<IPurchaseOrderFactory, CloudyPurchaseOrderFactory>();

            context.Services.AddSingleton<IOrderGroupBuilder, CloudyCartBuilder>();

            context.Services.AddSingleton<IOrderPostProcessor, DefaultOrderPostProcessor>();
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}

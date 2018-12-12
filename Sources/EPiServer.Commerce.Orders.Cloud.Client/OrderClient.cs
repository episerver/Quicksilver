using EPiServer.Commerce.Order;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using EPiServer.Commerce.OrderStore.Model;
using Mediachase.Commerce.Inventory;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public class OrderClient<T> where T : CloudyOrder
    {
        private readonly DocumentClient _client;
        readonly string _databaseId;
        private readonly int _throughput;

        private const string collectionId = "orders";

        public OrderClient(StorageSettings settings)
        {
            _databaseId = settings.DatabaseName;
            _throughput = settings.Throughput;

            _client = new DocumentClient(new Uri(settings.Endpoint), settings.AccountKey);
            EnsureResources().Wait();
        }

        public T Load(int orderGroupId)
        {
            try
            {
                var uri = UriFactory.CreateDocumentUri(_databaseId, collectionId, orderGroupId.ToString());
                var result =  _client.ReadDocumentAsync<OrderDocument>(uri, new RequestOptions() ).Result;

                return (T) ConvertToOrder(result);
            }
            catch (DocumentClientException)
            {
                return default(T);
            }

        }

        public IEnumerable<T> Load(Guid customerId, string name)
        {
            var options = new FeedOptions();
            IDocumentQuery<OrderDocument> query;
            if (string.IsNullOrEmpty(name))
            {
                query = _client.CreateDocumentQuery<OrderDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                    .Where(order => order.CustomerId == customerId).AsDocumentQuery();
            }
            else
            {
                query = _client.CreateDocumentQuery<OrderDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                    .Where(order => order.CustomerId == customerId && order.Name.ToLower() == name.ToLower()).AsDocumentQuery();
            }
           

            var result = query.ExecuteNextAsync<OrderDocument>().Result;

            return (IEnumerable<T>) result.Select(ConvertToOrder);

        }

        public T Load(string orderNumber)
        {
            var options = new FeedOptions( );
            var query = _client.CreateDocumentQuery<OrderDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                .Where(order => order.OrderNumber == orderNumber).AsDocumentQuery();

            var result = query.ExecuteNextAsync<OrderDocument>().Result;
            return (T) result.Select(ConvertToOrder).SingleOrDefault();

        }

        public IEnumerable<T> LoadIncompleteOrders()
        {
            var options = new FeedOptions();
            var query = _client.CreateDocumentQuery<OrderDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                .Where(order => !order.IsInventoryReserved).AsDocumentQuery();

            var orders = new List<CloudyOrder>();
            var response =  query.ExecuteNextAsync<OrderDocument>().Result;
            orders.AddRange(response.Select(ConvertToOrder));

            while (response.ResponseContinuation != null)
            {
                options.RequestContinuation = response.ResponseContinuation;
                response = query.ExecuteNextAsync<OrderDocument>().Result;
                orders.AddRange(response.Select(ConvertToOrder));
            }

            return (IEnumerable<T>) orders;

        }

        public T Save(IPurchaseOrder order)
        {
            var document = ConvertToDocument((CloudyOrder)order);

            var uri = UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId);

            if (order.OrderLink.OrderGroupId > 0)
            {
                _client.UpsertDocumentAsync(uri, document, new RequestOptions()).Wait();
            }
            else
            {
                document.OrderGroupId = GenerateId().ToString();
                _client.CreateDocumentAsync(uri, document, new RequestOptions() ).Wait();
            }

            return (T) ConvertToOrder(document);
        }

        public void Delete(int orderGroupId)
        {
            try
            {
                _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, collectionId, orderGroupId.ToString()), new RequestOptions() ).Wait();
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
        }

        OrderDocument ConvertToDocument(CloudyOrder order)
        {
            var serialized = JsonConvert.SerializeObject(order);
            var deserialized = JsonConvert.DeserializeObject<OrderDocument>(serialized);
            deserialized.OrderGroupId = order.OrderGroupId.ToString();

            deserialized.IsInventoryReserved = order.Forms.SelectMany(x => x.Shipments).SelectMany(x => x.LineItems)
                .All(x => x.IsInventoryAllocated || x.InventoryTrackingStatus == InventoryTrackingStatus.Disabled);
            return deserialized;
        }

        CloudyOrder ConvertToOrder(OrderDocument doc)
        {
            var serialized = JsonConvert.SerializeObject(doc);
            var deserialized = JsonConvert.DeserializeObject<CloudyOrder>(serialized);
            deserialized.OrderGroupId = int.Parse(doc.OrderGroupId);
            return deserialized;
        }

        static readonly Random _random = new Random();
        static readonly object _syncObject = new object();
        int GenerateId()
        {
            lock (_syncObject)
            {
                return _random.Next();
            }

        }

        private async Task EnsureResources()
        {
            var collection = new DocumentCollection { Id = collectionId };
            await _client.CreateDatabaseIfNotExistsAsync(new Database { Id = _databaseId });
            await _client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(_databaseId), collection, new RequestOptions { OfferThroughput = _throughput }).ConfigureAwait(false);
        }
    }
}

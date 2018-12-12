using EPiServer.Commerce.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using EPiServer.Commerce.OrderStore.Model;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public class CartClient<T> where T: CloudyCart
    {
        private readonly DocumentClient _client;
        private readonly string _databaseId;
        private readonly int _throughput;
        private const string collectionId = "carts";

        public CartClient(StorageSettings settings)
        {
            _databaseId = settings.DatabaseName;
            _throughput = settings.Throughput;
            _client = new DocumentClient(new Uri(settings.Endpoint), settings.AccountKey);


            EnsureResources().Wait();
        }

        public IEnumerable<T> Load(Guid customerId, string name)
        {
            var options = new FeedOptions();
            IDocumentQuery<CartDocument> query;
            if (string.IsNullOrEmpty(name))
            {
                query = _client.CreateDocumentQuery<CartDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                    .Where(cart => cart.CustomerId == customerId).AsDocumentQuery();

            }
            else
            {
                query = _client.CreateDocumentQuery<CartDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                    .Where(cart => cart.CustomerId == customerId && cart.Name.ToLower() == name.ToLower()).AsDocumentQuery();

            }

            var result = query.ExecuteNextAsync<CartDocument>().Result;

            return (IEnumerable<T>) result.Select(ConvertToCart);
        }

        public IEnumerable<T> Load(Guid customerId) 
        {
            var options = new FeedOptions ();
            var query = _client.CreateDocumentQuery<CartDocument>(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId), options)
                .Where(order => order.CustomerId == customerId).AsDocumentQuery();

            var result = query.ExecuteNextAsync<CartDocument>().Result;

            return (IEnumerable<T>) result.Select(ConvertToCart);

        }

        public T Load(int orderGroupId)
        {
            try
            {
                var uri = UriFactory.CreateDocumentUri(_databaseId, collectionId, orderGroupId.ToString());
                var result = _client.ReadDocumentAsync<CartDocument>(uri, new RequestOptions() ).Result;

                return  (T) ConvertToCart(result);
            }
            catch (DocumentClientException)
            {
                return default(T);
            }
        }

        public T Save(ICart cart)
        {
            var document = ConvertToDocument((CloudyCart)cart);
            var uri = UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId);

            if (cart.OrderLink.OrderGroupId > 0)
            {
                _client.UpsertDocumentAsync(uri, document, new RequestOptions() ).Wait();
            }
            else
            {
                document.OrderGroupId = GenerateId().ToString();

                _client.CreateDocumentAsync(uri, document, new RequestOptions() ).Wait();
            }

            return (T) ConvertToCart(document);
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

        CartDocument ConvertToDocument(CloudyCart order)
        {
            var serialized = JsonConvert.SerializeObject(order);
            var deserialized = JsonConvert.DeserializeObject<CartDocument>(serialized);
            deserialized.OrderGroupId = order.OrderGroupId.ToString();
            return deserialized;
        }

        CloudyCart ConvertToCart(CartDocument doc)
        {
            var serialized = JsonConvert.SerializeObject(doc);
            var deserialized = JsonConvert.DeserializeObject<CloudyCart>(serialized);
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

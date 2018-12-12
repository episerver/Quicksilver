using System.Configuration;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    public class StorageSettings  
    {
        const string EndpointKey = "episerver:cosmos.endpoint";
        const string AccountKeyKey = "episerver:cosmos.accountkey";
        const string DatabaseNameKey = "episerver:cosmos.databasename";
        const string ThroughputKey = "episerver:cosmos.throughput";

        public string Endpoint => ConfigurationManager.AppSettings[EndpointKey];
        public string AccountKey => ConfigurationManager.AppSettings[AccountKeyKey];
        public string DatabaseName => ConfigurationManager.AppSettings[DatabaseNameKey];
        public int Throughput => int.Parse(ConfigurationManager.AppSettings[ThroughputKey]);

    }
}

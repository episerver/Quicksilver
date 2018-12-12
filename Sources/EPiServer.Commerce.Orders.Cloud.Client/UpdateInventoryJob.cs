using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Commerce.Orders.Cloud.Client.OrderModel;
using EPiServer.Data;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;

namespace EPiServer.Commerce.Orders.Cloud.Client
{
    [ScheduledPlugIn(
        DisplayName = "Post process of orders",
        Description = "Processes incomplete orders that were taken during readonly mode")]
    [ServiceConfiguration(typeof(UpdateInventoryJob), Lifecycle = ServiceInstanceScope.Singleton)]
    class UpdateInventoryJob : ScheduledJobBase
    {
        static readonly ILogger _log = LogManager.GetLogger(typeof(UpdateInventoryJob));
        readonly IOrderPostProcessor _orderPostProcessor;
        readonly OrderClient<CloudyOrder> _orderClient;
        readonly IDatabaseMode _databaseMode;

        public UpdateInventoryJob(IOrderPostProcessor orderPostProcessor, OrderClient<CloudyOrder> orderClient, IDatabaseMode databaseMode)
        {
            _orderPostProcessor = orderPostProcessor;
            _orderClient = orderClient;
            _databaseMode = databaseMode;
        }

        public override string Execute()
        {
            return ProcessOrderFromService().Result;
        }

        async Task<string> ProcessOrderFromService()
        {
            var messages = new List<string>();

            _log.Information("Starting to process orders saved in read-only mode.");

            if (_databaseMode.DatabaseMode == DatabaseMode.ReadOnly)
            {
                var message = "Unable to process orders because this job is not supported in readonly mode.";
                _log.Information(message);
                return message;
            }

            foreach (var order in _orderClient.LoadIncompleteOrders())
            {
                var messagesForCurrentOrder = await _orderPostProcessor.ProcessOrderAsync(order);
                _log.Information(string.Join(Environment.NewLine, messagesForCurrentOrder));

                messages.AddRange(messagesForCurrentOrder);
            }

            return string.Join(Environment.NewLine, messages);
        }
    }
}
namespace RequestService
{
    using MassTransit;
    using MassTransit.AzureServiceBusTransport;
    using MassTransit.Util;
    using Microsoft.ServiceBus;
    using System;
    using System.Configuration;
    using Topshelf;
    using Topshelf.Logging;

    internal class RequestService :
        ServiceControl
    {
        private readonly LogWriter _log = HostLogger.Get<RequestService>();

        private IBusControl _busControl;

        public bool Start(HostControl hostControl)
        {
            _log.Info("Creating bus...");

            _busControl = Bus.Factory.CreateUsingAzureServiceBus(x =>
            {
                var host = x.Host(new Uri("sb://jperotest.servicebus.windows.net/"), h =>
                {
                    h.OperationTimeout = TimeSpan.FromSeconds(10);
                    h.TokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider("queue_mgr", "xxx");
                });

                x.ReceiveEndpoint(host, ConfigurationManager.AppSettings["ServiceQueueName"],
                    e => { e.Consumer<RequestConsumer>(); });
            });

            _log.Info("Starting bus...");

            TaskUtil.Await(() => _busControl.StartAsync());

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("Stopping bus...");

            _busControl?.Stop();

            return true;
        }
    }
}
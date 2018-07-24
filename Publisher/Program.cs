using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Persistence.Sql;

static class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.PubSub.Publisher";
        var endpointConfiguration = new EndpointConfiguration("Samples.PubSub.Publisher");

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        var connection = @"Data Source=localhost;Initial Catalog=Persistence;Integrated Security=True; Max Pool Size=100";
        persistence.SqlDialect<SqlDialect.MsSqlServer>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                // can do with EF
                return new SqlConnection(connection);
            });
        var subscriptions = persistence.SubscriptionSettings();
        subscriptions.CacheFor(TimeSpan.FromMinutes(10));

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString("Data Source=localhost;Initial Catalog=Transport;Integrated Security=True; Max Pool Size=100");

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();

        var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        await Start(endpointInstance).ConfigureAwait(false);
        await endpointInstance.Stop().ConfigureAwait(false);
    }

    static async Task Start(IEndpointInstance endpointInstance)
    {
        Console.WriteLine("Press '1' to publish the OrderReceived event");
        Console.WriteLine("Press any other key to exit");

        #region PublishLoop

        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();

            var orderReceivedId = Guid.NewGuid();
            if (key.Key == ConsoleKey.D1)
            {
                var orderReceived = new OrderReceived
                {
                    OrderId = orderReceivedId
                };
                await endpointInstance.Publish(orderReceived)
                    .ConfigureAwait(false);
                Console.WriteLine($"Published OrderReceived Event with Id {orderReceivedId}.");
            }
            else
            {
                return;
            }
        }

        #endregion
    }
}
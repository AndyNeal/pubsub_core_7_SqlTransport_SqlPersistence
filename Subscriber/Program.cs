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
        Console.Title = "Samples.PubSub.Subscriber";
        var endpointConfiguration = new EndpointConfiguration("Samples.PubSub.Subscriber");
        endpointConfiguration.UsePersistence<InMemoryPersistence>();

        //endpointConfiguration.UsePersistence<LearningPersistence>();
        //var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        //var connection = @"Data Source=localhost;Initial Catalog=Persistence;Integrated Security=True; Max Pool Size=100";
        //persistence.SqlDialect<SqlDialect.MsSqlServer>();
        //persistence.ConnectionBuilder(
        //    connectionBuilder: () =>
        //    {
        //        return new SqlConnection(connection);
        //    });
        //var subscriptions = persistence.SubscriptionSettings();
        //subscriptions.CacheFor(TimeSpan.FromMinutes(10));

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString("Data Source=localhost;Initial Catalog=Transport;Integrated Security=True; Max Pool Size=100");
        var routing = transport.Routing();
        routing.RegisterPublisher(assembly: typeof(OrderReceived).Assembly, publisherEndpoint: "Samples.PubSub.Publisher");

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();

        var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}
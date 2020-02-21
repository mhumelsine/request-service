using Automatonymous;
using EventStore;
using GreenPipes;
using GreenPipes.Introspection;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.Azure.ServiceBus.Core.Saga;
using MassTransit.TestFramework;
//using MassTransit.RedisIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RequestService.Aggregates;
using RequestService.Commands;
using RequestService.Events;
using RequestService.Sagas;
using System;
using System.Threading.Tasks;


namespace RequestService
{
    class Program
    {

        static void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        static async Task Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += OnException;
            //need config
            string serviceBusHostAddress = "localhost";
            string serviceBusHostUsername = "guest";
            string serviceBusHostPassword = "guest";
            string sqlServerConnectionString = "Data Source=merlindevsql01.isf.com;Initial Catalog=Merlin;Integrated Security=True";
            string serviceBusConnectionString = "Endpoint=sb://merlinsb.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=PPPXGlPcVO8lCVgJgi6laNWaR9hwcSXCPCqrKmmtv8U=";
            //string sqlServerConnectionString = @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=Merlin;Integrated Security=True";
            //string sqlServerConnectionString = "Data Source=doh-wddb005;Initial Catalog=Merlin;Integrated Security=True";

            IBusControl bus = null;

            var builder = new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<AggregateRepository<Request>>();
                    services.AddSingleton<IEventSerializer, JsonEventSerializer>();
                    services.AddSingleton<IEventStore, DapperEventStore>(provider =>
                        new DapperEventStore(sqlServerConnectionString, provider.GetRequiredService<IEventSerializer>()));

                    services.AddMassTransit(x =>
                    {
                        x.AddSagaStateMachine<RequestStateMachine, RequestState>()
                        .MessageSessionRepository();

                        x.AddConsumer<IdentifyFacilityConsumer>();
                        x.AddConsumer<IdentifyProviderConsumer>();
                        x.AddConsumer<RequestReceiveConsumer>();

                        x.AddBus(provider =>
                        {
                            bus = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
                            {
                                cfg.Publish<RequestReceived>(tcfg =>
                                {
                                    tcfg.EnablePartitioning = true;                                   
                                });

                                cfg.Send<RequestReceived>(tcfg =>
                                {
                                    tcfg.UseSessionIdFormatter(context => context.Message.CorrelationId.ToString("D"));
                                });

                                cfg.Send<IdentifyFacility>(tcfg =>
                                {
                                    tcfg.UseSessionIdFormatter(context => context.Message.CorrelationId.ToString("D"));
                                });

                                cfg.Send<IdentifyProvider>(tcfg =>
                                {
                                    tcfg.UseSessionIdFormatter(context => context.Message.CorrelationId.ToString("D"));
                                });

                                cfg.Host(serviceBusConnectionString);                                

                                cfg.ReceiveEndpoint("request", ecfg =>
                                {
                                    ecfg.RequiresSession = true;

                                    //may need for latency
                                    ecfg.PrefetchCount = 1000;

                                    ecfg.ConfigureSaga<RequestState>(provider);

                                    ecfg.ConfigureConsumer<IdentifyFacilityConsumer>(provider);
                                    ecfg.ConfigureConsumer<IdentifyProviderConsumer>(provider);
                                });
                            });


                            //ProbeResult result = bus.GetProbeResult();

                            //Console.WriteLine(result.ToJsonString());

                            return bus;
                        });


                    });


                    services.AddHostedService<Service>();
                    services.AddHostedService(provider => new QueuePoller(
                        TimeSpan.FromMilliseconds(10),
                        provider.GetRequiredService<AggregateRepository<Request>>()));
                });


            await builder.RunConsoleAsync();
        }
    }
}

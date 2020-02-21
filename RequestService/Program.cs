using Automatonymous;
using EventStore;
using GreenPipes;
using GreenPipes.Introspection;
using MassTransit;
using MassTransit.Azure.ServiceBus.Core;
using MassTransit.TestFramework;
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
    class ProgramExample
    {

        static void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        static async Task Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += OnException;

            string serviceBusConnectionString = "Secret";

            IBusControl bus = null;

            var builder = new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<AggregateRepository<Request>>();
                    services.AddSingleton<IEventSerializer, JsonEventSerializer>();
                    services.AddSingleton<IEventStore, InMemoryEventStore>();

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
                                cfg.Host(serviceBusConnectionString);

                                cfg.ReceiveEndpoint("request", ecfg =>
                                {
                                    ecfg.RequiresSession = true;

                                    //also tried this
                                    //ecfg.StateMachineSaga<RequestState>(provider);
                                    ecfg.ConfigureSaga<RequestState>(provider);

                                    ecfg.ConfigureConsumer<IdentifyFacilityConsumer>(provider);
                                    ecfg.ConfigureConsumer<IdentifyProviderConsumer>(provider);
                                    ecfg.ConfigureConsumer<RequestReceiveConsumer>(provider);
                                });
                            });

                            ProbeResult result = bus.GetProbeResult();

                            Console.WriteLine(result.ToJsonString());

                            return bus;
                        });
                    });

                    services.AddHostedService<Service>();

                    //this generated the first message the saga listens to
                    services.AddHostedService(provider => new QueuePoller(
                        TimeSpan.FromMilliseconds(1000),
                        provider.GetRequiredService<AggregateRepository<Request>>()));
                });

            await builder.RunConsoleAsync();
        }
    }
}

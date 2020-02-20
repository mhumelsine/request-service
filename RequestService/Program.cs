using Automatonymous;
using EventStore;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
//using MassTransit.RedisIntegration;
using MassTransit.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RequestService.Aggregates;
using RequestService.Commands;
using RequestService.Sagas;
using System;
using System.Threading.Tasks;


namespace RequestService
{
    class Program
    {

        static async Task Main(string[] args)
        {

            //need config
            string serviceBusHostAddress = "localhost";
            string serviceBusHostUsername = "guest";
            string serviceBusHostPassword = "guest";
            //string sqlServerConnectionString = "Data Source=merlindevsql01.isf.com;Initial Catalog=Merlin;Integrated Security=True";
            //string sqlServerConnectionString = @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=Merlin;Integrated Security=True";
            string sqlServerConnectionString = "Data Source=doh-wddb005;Initial Catalog=Merlin;Integrated Security=True";

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<AggregateRepository<Request>>();
                    services.AddSingleton<IEventSerializer, JsonEventSerializer>();
                    services.AddSingleton<IEventStore, DapperEventStore>(provider =>
                        new DapperEventStore(sqlServerConnectionString, provider.GetRequiredService<IEventSerializer>()));

                    //services.AddTransient<IEventStore, InMemoryEventStore>();

                    //services.AddTransient<>

                    //services.AddDbContext<RequestStateDbContext>(options =>
                    //{
                    //    options.UseSqlServer(sqlServerConnectionString);
                    //}, ServiceLifetime.Transient);

                    services.AddSingleton<SagaStateMachine<RequestState>, RequestStateMachine>();


                    //SagaStateMachine<RequestState>

                    //services.AddSingleton<ISagaRepository<RequestState>>(provider =>
                    //{
                    //    //return new DapperSagaRepository<RequestState>(sqlServerConnectionString);
                    //    return EntityFrameworkSagaRepository<RequestState>.CreateOptimistic(
                    //        () => provider.GetRequiredService<RequestStateDbContext>());

                    //    //var redis = ConnectionMultiplexer.Connect("merlin-bus.redis.cache.windows.net:6380,password=FTWpGZOqWYFuD7ODCWVStqZlnE7w6MxPsachuh8k+4U=,ssl=True,abortConnect=False");

                    //    //return new RedisSagaRepository<RequestState>(() => redis.GetDatabase());

                    //});

                    services.AddSingleton<ISagaRepository<RequestState>, InMemorySagaRepository<RequestState>>();


                    services.AddMassTransit(x =>
                    {
                        x.AddSagaStateMachine<RequestStateMachine, RequestState>()
                        //.InMemoryRepository();
                        //.RedisRepository(r =>
                        //{
                        //    r.DatabaseConfiguration("merlin-bus.redis.cache.windows.net:6380,password=FTWpGZOqWYFuD7ODCWVStqZlnE7w6MxPsachuh8k+4U=,ssl=True,abortConnect=False");
                        //    r.LockSuffix = "-locked";
                        //    r.LockTimeout = TimeSpan.FromMinutes(1);
                        //});
                        .EntityFrameworkRepository(repos =>
                        {
                            //repos.ConcurrencyMode = ConcurrencyMode.Optimistic;

                            repos.AddDbContext<RequestStateDbContext, RequestStateDbContext>((provider, options) =>
                            {
                                options.UseSqlServer(sqlServerConnectionString);
                                var logger = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Error));
                                options.UseLoggerFactory(logger);
                            });
                        });
                        //.Endpoint(ecfg =>
                        //{
                        //    ecfg.Name = "request-queue";
                        //    });

                        x.AddConsumer<IdentifyFacilityConsumer>();
                        x.AddConsumer<IdentifyProviderConsumer>();

                        x.AddBus(provider => Bus.Factory.CreateUsingInMemory(cfg =>
                        {
                            //var host = cfg.Host(serviceBusHostAddress, h =>
                            //{
                            //    h.Username(serviceBusHostUsername);
                            //    h.Password(serviceBusHostPassword);
                            //});

                            //cfg.PrefetchCount = 16;
                            cfg.ConfigureEndpoints(provider);


                            //    cfg.ReceiveEndpoint("request-queue", ecfg =>
                            //    {
                            //ecfg.UseInMemoryOutbox();
                            //        //ecfg.ConcurrencyLimit = 128;
                            //        //ecfg.UseMessageRetry(retry =>
                            //        //{
                            //        //    //retry.Incremental(10, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(10));
                            //        //    retry.Immediate(5);

                            //        //    //figure out what to do here
                            //        //    //https://masstransit-project.com/usage/exceptions.html#retry-configuration
                            //        //    //retry.Handle<Exception>();
                            //        //    //retry.Ignore<SqlException>();
                            //        //});

                            //        ecfg.Consumer(() => new IdentifyFacilityConsumer(provider.GetRequiredService<AggregateRepository<Request>>()));
                            //        ecfg.Consumer(() => new IdentifyProviderConsumer(provider.GetRequiredService<AggregateRepository<Request>>()));

                            //        //ecfg.StateMachineSaga<RequestState>(provider);

                            //        //ecfg.Consumer(() => new RequestReceiveConsumer());
                            //        //couldn't get this to work
                            //        //ecfg.ConfigureConsumer<IdentifyFacilityConsumer>(provider);
                            //        //ecfg.ConfigureConsumer<IdentifyProviderConsumer>(provider);
                            //        //ecfg.StateMachineSaga<RequestState>(provider);
                            //        //ecfg.StateMachineSaga(
                            //        //provider.GetRequiredService<SagaStateMachine<RequestState>>(),
                            //        //            provider.GetRequiredService<ISagaRepository<RequestState>>());

                            //        //ecfg.StateMachineSaga<RequestState>(provider);
                            //    });

                            //    //couldn't get this to work
                            //    //cfg.ConfigureEndpoints(provider);
                        }));
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

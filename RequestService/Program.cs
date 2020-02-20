﻿using Automatonymous;
using EventStore;
using GreenPipes;
using MassTransit;
using MassTransit.DapperIntegration;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Saga;
using MassTransit.RedisIntegration;
using MassTransit.RedisIntegration.Configuration;
using MassTransit.Saga;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RequestService.Aggregates;
using RequestService.Commands;
using RequestService.Events;
using RequestService.Sagas;
using StackExchange.Redis;
using System;
using System.Linq;
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
            string sqlServerConnectionString = "Data Source=merlindevsql01.isf.com;Initial Catalog=Merlin;Integrated Security=True";
            //string sqlServerConnectionString = @"Data Source=(localdb)\mssqllocaldb;Initial Catalog=Merlin;Integrated Security=True";
            //string sqlServerConnectionString = "Data Source=doh-wddb005;Initial Catalog=Merlin;Integrated Security=True";

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<AggregateRepository<Request>>();
                    services.AddSingleton<IEventSerializer, JsonEventSerializer>();
                    services.AddSingleton<IEventStore, DapperEventStore>(provider =>
                        new DapperEventStore(sqlServerConnectionString, provider.GetRequiredService<IEventSerializer>()));

                    //services.AddTransient<IEventStore, InMemoryEventStore>();

                    //services.AddTransient<>

                    services.AddDbContext<RequestStateDbContext>(options =>
                    {
                        options.UseSqlServer(sqlServerConnectionString);
                    }, ServiceLifetime.Transient);

                    services.AddTransient<SagaStateMachine<RequestState>, RequestStateMachine>();

                    //SagaStateMachine<RequestState>

                    //services.AddTransient<ISagaRepository<RequestState>>(provider =>
                    //{
                    //    //return new DapperSagaRepository<RequestState>(sqlServerConnectionString);
                    //    //return EntityFrameworkSagaRepository<RequestState>.CreateOptimistic(
                    //    //    () => provider.GetRequiredService<RequestStateDbContext>());

                    //    var redis = ConnectionMultiplexer.Connect("merlin-bus.redis.cache.windows.net:6380,password=FTWpGZOqWYFuD7ODCWVStqZlnE7w6MxPsachuh8k+4U=,ssl=True,abortConnect=False");

                    //    return new RedisSagaRepository<RequestState>(() => redis.GetDatabase());

                    //});

                    services.AddTransient<ISagaRepository<RequestState>, InMemorySagaRepository<RequestState>>();

                    //services.AddScoped<ISagaRepository<RequestState>>(provider => new InMemorySagaRepository<RequestState>());


                    services.AddMassTransit(x =>
                    {                       

                        //x.AddSagaStateMachine<RequestStateMachine, RequestState>()
                        //    .InMemoryRepository()
                        //    //.EntityFrameworkRepository(repos =>
                        //    //{
                        //    //    repos.ConcurrencyMode = ConcurrencyMode.Optimistic;

                        //    //    repos.AddDbContext<DbContext, RequestStateDbContext>((provider, options) =>
                        //    //    {
                        //    //        options.UseSqlServer(sqlServerConnectionString);
                        //    //    });
                        //    //})                            
                        //    .Endpoint(ecfg => {
                        //        ecfg.Name = "request-queue";                                
                        //    });


                        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            var host = cfg.Host(serviceBusHostAddress, h =>
                            {
                                h.Username(serviceBusHostUsername);
                                h.Password(serviceBusHostPassword);
                            });

                            //cfg.PrefetchCount = 16;

                            cfg.ReceiveEndpoint("request-queue", ecfg =>
                            {
                                ecfg.UseInMemoryOutbox();
                                //ecfg.ConcurrencyLimit = 128;
                                //ecfg.UseMessageRetry(retry =>
                                //{
                                //    //retry.Incremental(10, TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(10));
                                //    retry.Immediate(5);

                                //    //figure out what to do here
                                //    //https://masstransit-project.com/usage/exceptions.html#retry-configuration
                                //    //retry.Handle<Exception>();
                                //    //retry.Ignore<SqlException>();
                                //});

                                ecfg.Consumer(() => new IdentifyFacilityConsumer(provider.GetRequiredService<AggregateRepository<Request>>()));
                                ecfg.Consumer(() => new IdentifyProviderConsumer(provider.GetRequiredService<AggregateRepository<Request>>()));

                                //ecfg.StateMachineSaga<RequestState>(provider);

                                //ecfg.Consumer(() => new RequestReceiveConsumer());
                                //couldn't get this to work
                                //ecfg.ConfigureConsumer<IdentifyFacilityConsumer>(provider);
                                //ecfg.ConfigureConsumer<IdentifyProviderConsumer>(provider);
                                //ecfg.StateMachineSaga<RequestState>(provider);
                                //ecfg.StateMachineSaga(
                                //provider.GetRequiredService<RequestStateMachine>(),
                                //        provider.GetRequiredService<ISagaRepository<Request>>());

                                ecfg.StateMachineSaga<RequestState>(provider);
                            });

                            //couldn't get this to work
                            //cfg.ConfigureEndpoints(provider);
                        }));

                    });

                    services.AddHostedService<Service>();
                    services.AddHostedService(provider => new QueuePoller(
                        TimeSpan.FromMilliseconds(100),
                        provider.GetRequiredService<AggregateRepository<Request>>()));
                });


            await builder.RunConsoleAsync();
        }
    }
}

using EventStore;
using MassTransit;
using Microsoft.Extensions.Hosting;
using RequestService.Aggregates;
using RequestService.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RequestService
{
    public class QueuePoller : IHostedService
    {
        private readonly int pollingMilliseconds;
        private readonly AggregateRepository<Request> repos;

        public QueuePoller(TimeSpan pollingInterval, AggregateRepository<Request> repos)
        {
            this.pollingMilliseconds = (int)Math.Truncate(pollingInterval.TotalMilliseconds);
            this.repos = repos;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var s = new System.Diagnostics.Stopwatch();

            int i = 0;

            await Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    i++;
                    s.Reset();
                    s.Start();
                    await WorkerTask();
                    s.Stop();
                    await Task.Delay(pollingMilliseconds, cancellationToken);

                    Console.WriteLine($"Avg: {(sw.ElapsedMilliseconds / i)}: Current: {s.ElapsedMilliseconds}");
                    }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task WorkerTask()
        {
            //Console.WriteLine("poll");
            //Console.WriteLine(DateTime.Now);


            var request = new Request() { Uid = Guid.NewGuid() };

            request.Apply(new RequestReceived
            {
                CorrelationId = Guid.NewGuid(),
                FacilityName = "LabCrop",
                ProviderName = "Dr. Michael"
            });

            await repos.Save(request);
        }
    }
}

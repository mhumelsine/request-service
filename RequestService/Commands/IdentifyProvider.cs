using MassTransit;
using RequestService.Aggregates;
using System;
using EventStore;
using System.Collections.Generic;
using System.Text;
using RequestService.Events;
using System.Threading.Tasks;

namespace RequestService.Commands
{
    public class IdentifyProvider : CorrelatedBy<Guid>
    {
        public IdentifyProvider(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; set; }
    }

    public class IdentifyProviderConsumer : CommandConsumer<Request, IdentifyProvider, ProviderIdentified>
    {
        public IdentifyProviderConsumer(AggregateRepository<Request> repos) : base(repos)
        {
        }

        protected override Task<ProviderIdentified> Handle(ConsumeContext<IdentifyProvider> context, Request aggregate)
        {
            var @event = new ProviderIdentified { ProviderId = 44 };

            //Console.WriteLine("On Provider");

            return Task.FromResult(@event);
        }
    }
}

using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using EventStore;
using RequestService.Events;
using RequestService.Aggregates;
using System.Threading.Tasks;

namespace RequestService.Commands
{
    public class IdentifyFacility : CorrelatedBy<Guid>
    {
        public IdentifyFacility(Guid correlationId)
        {
            CorrelationId = correlationId;
        }
        public Guid CorrelationId { get; set; }
    }

    public class IdentifyFacilityConsumer : CommandConsumer<Request, IdentifyFacility, FacilityIdentified>
    {
        public IdentifyFacilityConsumer(AggregateRepository<Request> repos) : base(repos)
        {
        }

        protected override Task<FacilityIdentified> Handle(ConsumeContext<IdentifyFacility> context, Request aggregate)
        {
            //do work of resource matching
            var @event = new FacilityIdentified { FacilityId = 128 };

            //Console.WriteLine("On Facility");

            return Task.FromResult(@event);
        }
    }
}

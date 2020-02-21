using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public class AggregateRepository<TAggregate> where TAggregate : Aggregate, new()
    {
        private readonly IEventStore eventStore;
        private readonly IBus bus;

        public AggregateRepository(IEventStore eventStore, IBus bus)
        {
            this.eventStore = eventStore;
            this.bus = bus;
        }

        public async Task<TAggregate> Get(Guid uid)
        {
            var aggregate = new TAggregate { Uid = uid };

            var events = await eventStore.GetEventStream(uid);

            aggregate.Rehydrate(events);

            return aggregate;
        }

        public async Task Save(TAggregate aggregate)
        {
            var stream = aggregate.GetUncommittedEventStream();

            await eventStore.SaveEventStream(stream);

            aggregate.ResetEventStream();

            foreach (var @event in stream)
            {
                //this cannot be an abstract class because type is used for routing
                //need to invoke the generic type here
                await bus.Publish(@event, @event.GetType());
            }
        }
    }
}

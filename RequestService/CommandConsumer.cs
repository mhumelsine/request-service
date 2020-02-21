using MassTransit;
using System;
using System.Threading.Tasks;

namespace EventStore
{
    public abstract class CommandConsumer<TAggregate, TCommand, TEvent> : IConsumer<TCommand>
                where TCommand : class, CorrelatedBy<Guid>
                where TAggregate : Aggregate, new()
                where TEvent : Event
    {
        private readonly AggregateRepository<TAggregate> repos;

        public CommandConsumer(AggregateRepository<TAggregate> repos)
        {
            this.repos = repos;
        }

        public async Task Consume(ConsumeContext<TCommand> context)
        {
            var aggregate = await repos.Get(context.Message.CorrelationId);

            var @event = await Handle(context, aggregate);

            aggregate.Apply(@event);

            await repos.Save(aggregate);
        }

        protected abstract Task<TEvent> Handle(ConsumeContext<TCommand> context, TAggregate aggregate);
    }
}

using Automatonymous;
using Automatonymous.Requests;
using MassTransit.RedisIntegration;
using MassTransit.Saga;
using RequestService.Commands;
using RequestService.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestService.Sagas
{
    public class RequestState : 
        SagaStateMachineInstance
        //, IVersionedSaga
    {
        public string CurrentState { get; set; }
        public Guid CorrelationId { get; set; }
        public int ResourceMatchingEventStatus { get; set; }
        //public int Version { get; set; }
    }

    public class RequestStateMachine : MassTransitStateMachine<RequestState>
    {
        public RequestStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Initially(
                When(RequestReceived)
                .Publish(context =>new IdentifyProvider(context.Instance.CorrelationId))
                .Publish(context => new IdentifyFacility(context.Instance.CorrelationId))
                .Then(context =>
                {
                    Console.WriteLine(context.Event.Name);
                })
                .TransitionTo(ResourceMatchingState));

            CompositeEvent(() => ResourcesMatched, x => x.ResourceMatchingEventStatus, ProviderIdentified, FacilityIdentified);

            During(ResourceMatchingState,
                When(ResourcesMatched)
                //.Then(context => Console.WriteLine("Matched"))
                //.TransitionTo(ResourceMatchCompleteState));
                .Finalize());

            SetCompletedWhenFinalized();
        }

        public Event<RequestReceived> RequestReceived { get; set; }
        public Event<ProviderIdentified> ProviderIdentified { get; set; }
        public Event<FacilityIdentified> FacilityIdentified { get; set; }
        public Automatonymous.Event ResourcesMatched { get; set; }
        public State ProfileMatchedState { get; set; }
        public State ResourceMatchingState { get; set; }
        public State ResourceMatchCompleteState { get; set; }
    }
}

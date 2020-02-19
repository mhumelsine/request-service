using EventStore;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RequestService.Events
{
    public class RequestReceived : Event
    {
        public int ProviderId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string ProviderName { get; set; }
    }

    public class RequestReceiveConsumer : IConsumer<RequestReceived>
    {
        public Task Consume(ConsumeContext<RequestReceived> context)
        {
            return Task.CompletedTask;
        }
    }
}

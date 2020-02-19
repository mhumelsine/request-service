using EventStore;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestService.Events
{
    public class ProviderIdentified : Event
    {
        public int ProviderId { get; set; }
    }
}

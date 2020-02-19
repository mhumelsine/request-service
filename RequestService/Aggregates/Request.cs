using EventStore;
using RequestService.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestService.Aggregates
{
    public class Request : Aggregate
    {
        public int ProviderId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string ProviderName { get; set; }

        public void On(RequestReceived @event)
        {
            ProviderName = @event.ProviderName;
            FacilityName = @event.FacilityName;
        }

        public void On(FacilityIdentified @event)
        {
            FacilityId = @event.FacilityId;
        }

        public void On(ProviderIdentified @event)
        {
            ProviderId = @event.ProviderId;
        }
    }
}

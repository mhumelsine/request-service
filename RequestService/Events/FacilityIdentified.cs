using EventStore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestService.Events
{
    public class FacilityIdentified : Event
    {
        public int FacilityId { get; set; }
    }
}

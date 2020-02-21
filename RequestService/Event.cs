using MassTransit;
using MassTransit.Saga;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public abstract class Event : CorrelatedBy<Guid>
    {
        public Guid Uid { get; set; }
        public DateTime Timestamp { get; set; }

        public Guid CorrelationId { get { return Uid; } set { Uid = value; } }
    }
}

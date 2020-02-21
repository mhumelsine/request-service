using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStore
    {
        Task<List<Event>> GetEventStream(Guid uid);
        Task SaveEventStream(List<Event> stream);
    }
}

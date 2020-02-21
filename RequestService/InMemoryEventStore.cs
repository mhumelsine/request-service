using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public class InMemoryEventStore : IEventStore
    {
        private Dictionary<Guid, List<Event>> store = new Dictionary<Guid, List<Event>>();

        public Task<List<Event>> GetEventStream(Guid uid)
        {
            if (store.TryGetValue(uid, out var stream))
            {
                return Task.FromResult(stream);
            }

            throw new KeyNotFoundException();
        }

        public Task SaveEventStream(List<Event> stream)
        {
            //nothing to commit
            if (!stream.Any())
            {
                return Task.CompletedTask;
            }

            var e = stream.First();

            if (store.TryGetValue(e.Uid, out var current))
            {
                current.AddRange(stream);
            }
            else
            {
                store.Add(e.Uid, stream.ToList());
            }

            return Task.CompletedTask;
        }
    }
}

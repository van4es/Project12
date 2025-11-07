using FixItNR.Api.Models;
using System.Collections.Concurrent;


namespace FixItNR.Api.Data
{
    public class InMemoryTicketRepository : ITicketRepository
    {
        private readonly ConcurrentDictionary<Guid, Ticket> _store = new();


        public IEnumerable<Ticket> GetAll() => _store.Values.OrderByDescending(t => t.CreatedAt);
        public Ticket? GetById(Guid id) => _store.TryGetValue(id, out var t) ? t : null;
        public Ticket Add(Ticket t) { _store[t.Id] = t; return t; }
    }

}
using FixItNR.Api.Models;
using System.Collections.Concurrent;


namespace FixItNR.Api.Data
{
    public interface ITicketRepository
    {
        IEnumerable<Ticket> GetAll();
        Ticket? GetById(Guid id);
        Ticket Add(Ticket ticket);
    }

}
using ServerApp.Domain.Data;
using ServerApp.Domain.Models;

namespace ServerApp.Contracts.Repositories;
public interface ITicketRepository : IRepositoryBase<Ticket, SqlServerDbContext>
{
}

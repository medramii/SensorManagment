using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.Data;
using ServerApp.Domain.Models;
using ServerApp.Persistence.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Persistence.Repositories;
public class TicketRepository : RepositoryBase<Ticket, SqlServerDbContext>, ITicketRepository
{
    public TicketRepository(SqlServerDbContext context, IDistributedCache cache, IConfiguration config) : base(context, cache, config)
    {
    }
}

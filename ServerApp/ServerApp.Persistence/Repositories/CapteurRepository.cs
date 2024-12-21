using Microsoft.EntityFrameworkCore;
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
public class CapteurRepository<TContext> 
    : RepositoryBase<Capteur,TContext>,
    ICapteurRepository<TContext>
    where TContext : DbContext
{
    public CapteurRepository(TContext context, IDistributedCache cache, IConfiguration config) : base(context, cache, config)
    {
    }
}

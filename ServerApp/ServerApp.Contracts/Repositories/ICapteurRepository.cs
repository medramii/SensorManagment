using ServerApp.Domain.Models;

namespace ServerApp.Contracts.Repositories;
public interface ICapteurRepository<TContext> : IRepositoryBase<Capteur, TContext>
{
}

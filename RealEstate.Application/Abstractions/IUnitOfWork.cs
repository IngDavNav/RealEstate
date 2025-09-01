using System.Data;
using System.Threading.Tasks;
using System.Threading;
using RealEstate.Application.Abstractions.Repositories;

namespace RealEstate.Application.Abstractions
{
    public interface IUnitOfWork
    {
        IOwnerRepository Owners { get; }
        IPropertyRepository Properties { get; }

        Task<IDbTransaction> BeginAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(IDbTransaction tx, CancellationToken cancellationToken = default);
        Task RollbackAsync(IDbTransaction tx);
    }
}

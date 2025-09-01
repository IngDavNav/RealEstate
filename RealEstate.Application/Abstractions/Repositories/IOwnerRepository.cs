using RealEstate.Domain.Models;

using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.Abstractions.Repositories
{
    public interface IOwnerRepository
    {
        Task<Owner?> GetAsync(int idOwner, CancellationToken cancellationToken);
        Task<bool> Exists(int idOwner, CancellationToken cancellationToken);
    }
}

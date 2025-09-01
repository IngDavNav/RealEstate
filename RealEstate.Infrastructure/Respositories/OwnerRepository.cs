using Dapper;

using Microsoft.EntityFrameworkCore;

using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Domain.Models;
using RealEstate.Infrastructure.Context;
using RealEstate.Infrastructure.Data;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Infrastructure.Repositories;

public class OwnerRepository(RealEstateDbContext db) : IOwnerRepository
{
    public async Task<Owner> GetAsync(int idOwner, CancellationToken cancellationToken)
    {
        return await db.Owners.AsNoTracking()
                               .FirstOrDefaultAsync(o => o.IdOwner == idOwner, cancellationToken);
    }

    public Task<bool> Exists(int idOwner, CancellationToken cancellationToken)
    {
        return db.Owners.AsNoTracking().AnyAsync(o => o.IdOwner == idOwner, cancellationToken);
    }
}

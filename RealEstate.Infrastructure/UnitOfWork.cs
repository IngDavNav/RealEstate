using Microsoft.EntityFrameworkCore.Storage;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Infrastructure.Context;

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly RealEstateDbContext _db;
    private IDbContextTransaction? _tx;

    public IOwnerRepository Owners { get; }
    public IPropertyRepository Properties { get; }

    public UnitOfWork(RealEstateDbContext db, IOwnerRepository owners, IPropertyRepository properties)
    {
        _db = db;
        Owners = owners;
        Properties = properties;
    }

    public async Task<IDbTransaction> BeginAsync(CancellationToken cancellationToken = default)
    {
        if (_tx is not null) throw new InvalidOperationException("Transaction already started.");
        _tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        return _tx.GetDbTransaction();
    }

    public async Task CommitAsync(IDbTransaction tx, CancellationToken cancellationToken = default)
    {
        if (_tx is null || !ReferenceEquals(_tx.GetDbTransaction(), tx))
            throw new InvalidOperationException("Transaction mismatch.");

        await _db.SaveChangesAsync(cancellationToken);

        await _tx.CommitAsync(cancellationToken);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackAsync(IDbTransaction tx)
    {
        if (_tx is not null && ReferenceEquals(_tx.GetDbTransaction(), tx))
        {
            await _tx.RollbackAsync();
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx is not null)
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }
}

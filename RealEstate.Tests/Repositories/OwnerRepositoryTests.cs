using Microsoft.EntityFrameworkCore;
using RealEstate.Domain.Models;
using RealEstate.Infrastructure.Context;
using RealEstate.Infrastructure.Repositories;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RealEstate.Tests.Repositories;

public class OwnerRepositoryTests
{
    private RealEstateDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new RealEstateDbContext(options);
    }

    [Fact]
    public async Task GetAsync_OwnerExists_ReturnsOwner()
    {
        // Arrange
        var dbName = nameof(GetAsync_OwnerExists_ReturnsOwner);
        using var db = CreateDbContext(dbName);
        var owner = new Owner { Name = "John Doe" };
        db.Owners.Add(owner);
        await db.SaveChangesAsync();

        var repo = new OwnerRepository(db);

        // Act
        var result = await repo.GetAsync(owner.IdOwner, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(owner.IdOwner, result.IdOwner);
    }

    [Fact]
    public async Task GetAsync_OwnerDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dbName = nameof(GetAsync_OwnerDoesNotExist_ReturnsNull);
        using var db = CreateDbContext(dbName);
        var repo = new OwnerRepository(db);

        // Act
        var result = await repo.GetAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Exists_OwnerExists_ReturnsTrue()
    {
        // Arrange
        var dbName = nameof(Exists_OwnerExists_ReturnsTrue);
        using var db = CreateDbContext(dbName);
        var owner = new Owner { Name = "Jane Doe" };
        db.Owners.Add(owner);
        await db.SaveChangesAsync();

        var repo = new OwnerRepository(db);

        // Act
        var exists = await repo.Exists(owner.IdOwner, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task Exists_OwnerDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dbName = nameof(Exists_OwnerDoesNotExist_ReturnsFalse);
        using var db = CreateDbContext(dbName);
        var repo = new OwnerRepository(db);

        // Act
        var exists = await repo.Exists(12345, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }
}

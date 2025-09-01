using Microsoft.EntityFrameworkCore;

using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Domain.Models;
using RealEstate.Infrastructure.Context;
using RealEstate.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace RealEstate.Tests.Repositories;

public class PropertyRepositoryTests
{
    private RealEstateDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<RealEstateDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new RealEstateDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_AddsPropertyAndReturnsIt()
    {
        // Arrange
        var dbName = nameof(CreateAsync_AddsPropertyAndReturnsIt);
        using var db = CreateDbContext(dbName);
        var repo = new PropertyRepository(db);
        var property = new Property { Name = "Casa", Price = 100, IdOwner = 1 };

        // Act
        var result = await repo.CreateAsync(property, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Casa", result.Name);
        Assert.True(db.Properties.Any(p => p.IdProperty == result.IdProperty));
    }

    [Fact]
    public async Task GetPropertyByFiltersAsync_ReturnsPagedFilteredProperties()
    {
        // Arrange
        var dbName = nameof(GetPropertyByFiltersAsync_ReturnsPagedFilteredProperties);
        using var db = CreateDbContext(dbName);
        db.Properties.AddRange(
            new Property { Name = "A", Price = 100, Year = 2020, Address = new PropertyAddress { Street = "Main", City = "X", State = "Y", ZipCode = "123" }, IdOwner = 1 },
            new Property { Name = "B", Price = 200, Year = 2021, Address = new PropertyAddress { Street = "Second", City = "X", State = "Y", ZipCode = "123" }, IdOwner = 1}
        );
        await db.SaveChangesAsync();

        var repo = new PropertyRepository(db);

        var filters = new PropertyFilters
        {
            Address = new AddressDto { Street = "Main" },
            MinPrice = 50,
            MaxPrice = 150,
        };

        // Act
        var result = await repo.GetPropertyByFiltersAsync(filters, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("A", result.Items[0].Name);
    }

    [Fact]
    public async Task GetPropertyByFiltersAsync_EmptyResult_ReturnsEmptyPagedDtos()
    {
        // Arrange
        var dbName = nameof(GetPropertyByFiltersAsync_EmptyResult_ReturnsEmptyPagedDtos);
        using var db = CreateDbContext(dbName);
        var repo = new PropertyRepository(db);

        var filters = new PropertyFilters
        {
            Address = new AddressDto { Street = "Nonexistent" },
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await repo.GetPropertyByFiltersAsync(filters, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public async Task GetDetailAsync_PropertyExists_ReturnsPropertyWithOwnerImagesTraces()
    {
        // Arrange
        var dbName = nameof(GetDetailAsync_PropertyExists_ReturnsPropertyWithOwnerImagesTraces);
        using var db = CreateDbContext(dbName);
        var owner = new Owner { Name = "Owner" };
        var property = new Property
        {
            Name = "Prop",
            Price = 100,
            IdOwner = 1,
            Owner = owner,
            Images = new List<PropertyImage> { new PropertyImage { File = "img.jpg", Enabled = true } },
            Traces = new List<PropertyTrace> { new PropertyTrace { Name = "Venta", Value = 100, DateSale = DateOnly.FromDateTime(DateTime.Today) } }
        };
        db.Owners.Add(owner);
        db.Properties.Add(property);
        await db.SaveChangesAsync();
        var repo = new PropertyRepository(db);

        // Act
        var result = await repo.GetDetailAsync(property.IdProperty, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Prop", result.Name);
        Assert.NotNull(result.Owner);
        Assert.NotEmpty(result.Images);
        Assert.NotEmpty(result.Traces);
    }

    [Fact]
    public async Task GetDetailAsync_PropertyDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dbName = nameof(GetDetailAsync_PropertyDoesNotExist_ReturnsNull);
        using var db = CreateDbContext(dbName);
        var repo = new PropertyRepository(db);

        // Act
        var result = await repo.GetDetailAsync(999, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ChangePriceAsync_PropertyExists_ChangesPriceAndReturnsSaveCount()
    {
        // Arrange
        var dbName = nameof(ChangePriceAsync_PropertyExists_ChangesPriceAndReturnsSaveCount);
        using var db = CreateDbContext(dbName);
        var property = new Property { Name = "Prop", Price = 100, IdOwner = 1 };
        db.Properties.Add(property);
        await db.SaveChangesAsync();
        var repo = new PropertyRepository(db);

        // Act
        var result = await repo.ChangePriceAsync(property.IdProperty, 200, CancellationToken.None);

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(200, db.Properties.First().Price);
    }

    [Fact]
    public async Task ChangePriceAsync_PropertyDoesNotExist_ReturnsZero()
    {
        // Arrange
        var dbName = nameof(ChangePriceAsync_PropertyDoesNotExist_ReturnsZero);
        using var db = CreateDbContext(dbName);
        var repo = new PropertyRepository(db);

        // Act
        var result = await repo.ChangePriceAsync(999, 200, CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
    }

    
    [Fact]
    public async Task UpdateAsync_PropertyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dbName = nameof(UpdateAsync_PropertyDoesNotExist_ReturnsFalse);
        using var db = CreateDbContext(dbName);
        var repo = new PropertyRepository(db);

        var updated = new Property
        {
            IdProperty = 999,
            Name = "NoProp",
            Price = 100,
            IdOwner = 1
        };

        // Act
        var result = await repo.UpdateAsync(updated, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddImageAsync_AddsImageAndReturnsId()
    {
        // Arrange
        var dbName = nameof(AddImageAsync_AddsImageAndReturnsId);
        using var db = CreateDbContext(dbName);
        var property = new Property { Name = "Prop", Price = 100, IdOwner = 1 };
        db.Properties.Add(property);
        await db.SaveChangesAsync();
        var repo = new PropertyRepository(db);

        var image = new PropertyImage { IdProperty = property.IdProperty, File = "img.jpg", Enabled = true };

        // Act
        var id = await repo.AddImageAsync(image, CancellationToken.None);

        // Assert
        Assert.True(id > 0);
        Assert.Single(db.PropertyImages);
        Assert.Equal("img.jpg", db.PropertyImages.First().File);
    }

    [Fact]
    public async Task Exists_PropertyExists_ReturnsTrue()
    {
        // Arrange
        var dbName = nameof(Exists_PropertyExists_ReturnsTrue);
        using var db = CreateDbContext(dbName);
        var property = new Property { Name = "Prop", Price = 100, IdOwner = 1 };
        db.Properties.Add(property);
        await db.SaveChangesAsync();
        var repo = new PropertyRepository(db);

        // Act
        var exists = await repo.Exists(property.IdProperty, CancellationToken.None);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task Exists_PropertyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dbName = nameof(Exists_PropertyDoesNotExist_ReturnsFalse);
        using var db = CreateDbContext(dbName);
        var repo = new PropertyRepository(db);

        // Act
        var exists = await repo.Exists(999, CancellationToken.None);

        // Assert
        Assert.False(exists);
    }
}
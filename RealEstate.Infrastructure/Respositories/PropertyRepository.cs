using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Domain.Models;
using RealEstate.Infrastructure.Context;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly RealEstateDbContext _db;

    public PropertyRepository(RealEstateDbContext db)
    {
        _db = db;
    }

    public async Task<Property> CreateAsync(Property property, CancellationToken cancelationToken)
    {
        _db.Properties.Add(property);
        await _db.SaveChangesAsync(cancelationToken);
        return property;
    }

    public async Task<PagedDtos<Property>> GetPropertyByFiltersAsync(PropertyFilters filters, CancellationToken cancelationToken)
    {
        var page = filters.Page < 1 ? 1 : filters.Page;
        var size = filters.PageSize <= 0 ? 20 : (filters.PageSize > 100 ? 100 : filters.PageSize);

        IQueryable<Property> propertiesList = _db.Properties;

        if (filters.Address is not null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Address.Street))
                propertiesList = propertiesList.Where(p => p.Address != null &&
                                 EF.Functions.Like(p.Address!.Street!, $"%{filters.Address.Street}%"));

            if (!string.IsNullOrWhiteSpace(filters.Address.City))
                propertiesList = propertiesList.Where(p => p.Address != null &&
                                 EF.Functions.Like(p.Address!.City!, $"%{filters.Address.City}%"));

            if (!string.IsNullOrWhiteSpace(filters.Address.State))
                propertiesList = propertiesList.Where(p => p.Address != null &&
                                 EF.Functions.Like(p.Address!.State!, $"%{filters.Address.State}%"));

            if (!string.IsNullOrWhiteSpace(filters.Address.ZipCode))
                propertiesList = propertiesList.Where(p => p.Address != null &&
                                 EF.Functions.Like(p.Address!.ZipCode!, $"%{filters.Address.ZipCode}%"));
        }

        if (filters.MinPrice.HasValue) propertiesList = propertiesList.Where(p => p.Price >= filters.MinPrice);
        if (filters.MaxPrice.HasValue) propertiesList = propertiesList.Where(p => p.Price <= filters.MaxPrice);
        if (filters.Year.HasValue) propertiesList = propertiesList.Where(p => p.Year == filters.Year);

        if (filters.HasImage)
        {
            propertiesList = propertiesList.Where(p => _db.PropertyImages.Any(i => i.IdProperty == p.IdProperty && i.Enabled));
        }

        var total = await propertiesList.CountAsync(cancelationToken);

        var items = await propertiesList
                           .OrderBy(p => p.Price)
                           .ThenBy(p => p.IdProperty)
                           .Skip((page - 1) * size)
                           .Take(size)
                           .ToListAsync(cancelationToken);

        return new PagedDtos<Property>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = size
        };
    }

    public async Task<Property?> GetDetailAsync(int idProperty, CancellationToken cancelationToken)
    {
        var property = await _db.Properties.AsNoTracking()
        .Include(p => p.Owner)
        .Include(p => p.Images)
        .Include(p => p.Traces)
        .AsSingleQuery()
        .FirstOrDefaultAsync(p => p.IdProperty == idProperty, cancelationToken);

        if (property is null) return null;

        return property;
    }

    public async Task<int> ChangePriceAsync(int idProperty, decimal newPrice, CancellationToken cancelationToken)
    {
        var prop = await _db.Properties.FirstOrDefaultAsync(p => p.IdProperty == idProperty, cancelationToken);
        if (prop is null) return 0;

        prop.Price = newPrice;
        return await _db.SaveChangesAsync(cancelationToken);
    }

    public async Task<bool> UpdateAsync(Property input, CancellationToken ct)
    {
        var existing = await _db.Properties
            .FirstOrDefaultAsync(p => p.IdProperty == input.IdProperty, ct);

        if (existing is null) return false;

        var priceChanged = existing.Price != input.Price;

        // actualizar campos
        existing.Name = input.Name;
        existing.Address = input.Address;
        existing.Price = input.Price;
        existing.CodeInternal = input.CodeInternal;
        existing.Year = input.Year;
        existing.IdOwner = input.IdOwner;

        if (priceChanged)
        {
            _db.PropertyTraces.Add(new PropertyTrace
            {
                IdProperty = existing.IdProperty,
                DateSale = DateOnly.FromDateTime(DateTime.UtcNow),
                Name = "Price updated",
                Value = input.Price,
                Tax = 0m
            });
        }

        return true;
    }

    public async Task<int> AddImageAsync(PropertyImage image, CancellationToken cancellationToken)
    {
        _db.PropertyImages.Add(image);
        await _db.SaveChangesAsync(cancellationToken);
        return image.IdPropertyImage;
    }

    public Task<bool> Exists(int idProperty, CancellationToken cancellationToken)
    {
        return _db.Properties.AsNoTracking().AnyAsync(o => o.IdProperty == idProperty, cancellationToken);
    }
}

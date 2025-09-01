using RealEstate.Domain.Models;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Threading;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.Contracts.Commons;

namespace RealEstate.Application.Abstractions.Repositories;

public interface IPropertyRepository
{
    Task<Property> CreateAsync(Property property, CancellationToken cancellationToken);
    Task<PagedDtos<Property>> GetPropertyByFiltersAsync(PropertyFilters filters, CancellationToken cancellationToken);
    Task<Property?> GetDetailAsync(int idProperty, CancellationToken cancellationToken);
    Task<int> ChangePriceAsync(int idProperty, decimal newPrice, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Property input, CancellationToken ct);
    Task<int> AddImageAsync(PropertyImage image, CancellationToken cancellationToken);
    Task<bool> Exists(int idProperty, CancellationToken cancellationToken);
}

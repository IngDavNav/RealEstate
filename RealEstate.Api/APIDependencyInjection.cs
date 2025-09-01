using Microsoft.Extensions.DependencyInjection;

using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.GetPropertyDetails;
using RealEstate.Application.Contracts;
using RealEstate.Application.UseCases.GetPropertyList;
using RealEstate.Application.UseCases.ChangePrice;

namespace RealEstate.Api.Extensions;

public static class APIDependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        return services;
    }
}

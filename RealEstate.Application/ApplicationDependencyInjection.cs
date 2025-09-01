using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Files;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.AddPropertyImage;
using RealEstate.Application.UseCases.ChangePrice;
using RealEstate.Application.UseCases.CreatePropertyBuilding;
using RealEstate.Application.UseCases.GetPropertyDetails;
using RealEstate.Application.UseCases.GetPropertyList;
using RealEstate.Application.UseCases.UpdateProperty;

using System;

namespace RealEstate.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });

        services.AddScoped<IImageUrlBuilder, LocalImageUrlBuilder>();

        services.AddScoped<IQueryHandler<GetPropertyDetailsQuery, PropertyDetailDto>, GetPropertyDetailsHandler>();
        services.AddScoped<IQueryHandler<GetPropertyListQuery, PagedDtos<PropertySummaryDto>>, GetPropertyListHandler>();
        services.AddScoped<ICommandHandler<CreatePropertyBuildingCommand, PropertyDetailDto>, CreatePropertyBuildingHandler>();
        services.AddScoped<ICommandHandler<ChangePriceCommand, bool>, ChangePriceHandler>();
        services.AddScoped<ICommandHandler<UpdatePropertyCommand, bool>, UpdatePropertyHandler>();
        services.AddScoped<ICommandHandler<AddPropertyImageCommand, PropertyImageDto>, AddPropertyImageHandler>();

        return services;
    }
}

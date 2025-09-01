using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Files;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Infrastructure.Context;
using RealEstate.Infrastructure.Files;
using RealEstate.Infrastructure.Repositories;

namespace RealEstate.Infrastructure.Extensions;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ImageStorageOptions>(configuration.GetSection("ImageStorage"));
        services.AddScoped<IImageStorage, LocalImageStorage>();


        services.AddDbContext<RealEstateDbContext>(opt =>
           opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IOwnerRepository, OwnerRepository>();
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}

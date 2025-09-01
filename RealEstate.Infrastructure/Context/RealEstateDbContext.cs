using Microsoft.EntityFrameworkCore;

using RealEstate.Domain.Models;

namespace RealEstate.Infrastructure.Context;

public class RealEstateDbContext : DbContext
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();
    public DbSet<PropertyTrace> PropertyTraces => Set<PropertyTrace>();

    public RealEstateDbContext(DbContextOptions<RealEstateDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(RealEstateDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RealEstate.Domain.Models;

namespace RealEstate.Infrastructure.Context.Configurations;
public sealed class OwnerConfig : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> e)
    {
        e.ToTable("Owner");
        e.HasKey(x => x.IdOwner);
        e.Property(x => x.Name).IsRequired().HasMaxLength(255);
        e.Property(x => x.Photo).HasMaxLength(500);

        e.OwnsOne(x => x.Address, addr =>
        {
            addr.Property(p => p.Street).HasColumnName("Street").HasMaxLength(150).IsRequired(false);
            addr.Property(p => p.City).HasColumnName("City").HasMaxLength(100).IsRequired(false);
            addr.Property(p => p.State).HasColumnName("State").HasMaxLength(100).IsRequired(false);
            addr.Property(p => p.ZipCode).HasColumnName("ZipCode").HasMaxLength(20).IsRequired(false);

            addr.HasIndex(p => new { p.City, p.State });
        });

        e.Navigation(x => x.Address).IsRequired(false);
    }
}
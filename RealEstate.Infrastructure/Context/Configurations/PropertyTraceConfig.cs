using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RealEstate.Domain.Models;

namespace RealEstate.Infrastructure.Data.Configurations;

public sealed class PropertyTraceConfig : IEntityTypeConfiguration<PropertyTrace>
{
    public void Configure(EntityTypeBuilder<PropertyTrace> e)
    {
        e.ToTable("PropertyTrace");
        e.HasKey(x => x.IdPropertyTrace);
        e.Property(x => x.Name).IsRequired().HasMaxLength(255);
        e.Property(x => x.Value).HasColumnType("decimal(18,2)");
        e.Property(x => x.Tax).HasColumnType("decimal(18,2)");
        e.HasIndex(x => new { x.IdProperty, x.DateSale });
    }
}

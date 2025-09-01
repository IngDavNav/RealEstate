using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RealEstate.Domain.Models;

namespace RealEstate.Infrastructure.Context.Configurations
{
    public class PropertyImageConfig: IEntityTypeConfiguration<PropertyImage>
    {
        public void Configure(EntityTypeBuilder<PropertyImage> e)
        {
            e.ToTable("PropertyImage");
            e.HasKey(x => x.IdPropertyImage);
            e.Property(x => x.File).IsRequired().HasMaxLength(500);
            e.Property(x => x.Enabled).HasDefaultValue(true);
            e.HasIndex(x => new { x.IdProperty, x.Enabled });
        }
    }
}

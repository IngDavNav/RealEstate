using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RealEstate.Domain.Models;

namespace RealEstate.Infrastructure.Context.Configurations
{
    public class PropertyConfig : IEntityTypeConfiguration<Property>
    {
        public void Configure(EntityTypeBuilder<Property> e)
        {
            e.ToTable("Property");
            e.HasKey(x => x.IdProperty);

            e.Property(x => x.Name).IsRequired().HasMaxLength(255);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.CodeInternal).HasMaxLength(50);

            e.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.IdOwner)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Property_Owner");

            e.OwnsOne(x => x.Address, addr =>
            {
                addr.Property(p => p.Street).HasColumnName("Street").HasMaxLength(150).IsRequired(false);
                addr.Property(p => p.City).HasColumnName("City").HasMaxLength(100).IsRequired(false);
                addr.Property(p => p.State).HasColumnName("State").HasMaxLength(100).IsRequired(false);
                addr.Property(p => p.ZipCode).HasColumnName("ZipCode").HasMaxLength(20).IsRequired(false);

                addr.HasIndex(p => new { p.City, p.State });
            });

            e.Navigation(x => x.Address).IsRequired(false);

            e.HasIndex(x => x.Price);
            e.HasIndex(x => x.Year);

            e.HasMany(p => p.Images).WithOne().HasForeignKey(i => i.IdProperty)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.Traces).WithOne().HasForeignKey(t => t.IdProperty)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

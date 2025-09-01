using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using RealEstate.Domain.Models;
using RealEstate.Infrastructure.Context;

using System;

using System.Threading;

using System.Threading.Tasks;

public static class DbInitializer
{
    public static async Task CreateAndSeedAsync(this IServiceProvider sp, bool reset = false, CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RealEstateDbContext>();

        if (!await db.Database.CanConnectAsync(ct))
            throw new InvalidOperationException("No puedo conectar a RealEstateDb. Crea la base y da permisos al usuario.");

        if (reset) await db.Database.EnsureDeletedAsync(ct);
        var created = await db.Database.EnsureCreatedAsync(ct);
        if (!created && await db.Properties.AnyAsync(ct)) return;

        // --- OWNERS (Miami/área) ---
        var o1 = new Owner
        {
            Name = "Sofía Martínez",
            Address = new OwnerAddress { Street = "68 SE 6th St Apt 3501", City = "Miami", State = "FL", ZipCode = "33131" },
            Birthday = new DateOnly(1988, 3, 15)
        };
        var o2 = new Owner
        {
            Name = "Carlos Rivera",
            Address = null,
            Birthday = new DateOnly(1982, 11, 2)
        };
        var o3 = new Owner
        {
            Name = "Emily Johnson",
            Address = new OwnerAddress { Street = "4100 Salzedo St", City = "Coral Gables", State = "FL", ZipCode = "33146" },
            Birthday = new DateOnly(1991, 7, 7)
        };

        db.Owners.AddRange(o1, o2, o3);
        await db.SaveChangesAsync(ct);

        // --- PROPERTIES ---
        var p1 = new Property
        {
            Name = "Brickell High-Rise 2BR",
            Address = new PropertyAddress { Street = "68 SE 6th St", City = "Miami", State = "FL", ZipCode = "33131" },
            Price = 850_000m,
            CodeInternal = "BRK-2BR-6806",
            Year = 2019,
            IdOwner = o1.IdOwner,
            Owner = o1
        };
        var p2 = new Property
        {
            Name = "Wynwood Loft 1BR",
            Address = new PropertyAddress { Street = "2400 NW 2nd Ave", City = "Miami", State = "FL", ZipCode = "33127" },
            Price = 520_000m,
            CodeInternal = "WYN-LOFT-2400",
            Year = 2016,
            IdOwner = o2.IdOwner,
            Owner = o2
        };
        var p3 = new Property
        {
            Name = "South Beach Condo",
            Address = new PropertyAddress { Street = "1100 West Ave", City = "Miami Beach", State = "FL", ZipCode = "33139" },
            Price = 1_200_000m,
            CodeInternal = "SOBE-1100",
            Year = 2012,
            IdOwner = o3.IdOwner,
            Owner = o3
        };
        var p4 = new Property
        {
            Name = "Coral Gables Family Home",
            Address = new PropertyAddress { Street = "1200 Alhambra Cir", City = "Coral Gables", State = "FL", ZipCode = "33134" },
            Price = 1_050_000m,
            CodeInternal = "CG-ALH-1200",
            Year = 2008,
            IdOwner = o3.IdOwner,
            Owner = o3
        };
        var p5 = new Property
        {
            Name = "Little Havana Duplex",
            Address = new PropertyAddress { Street = "1000 SW 8th St", City = "Miami", State = "FL", ZipCode = "33130" },
            Price = 690_000m,
            CodeInternal = "LH-1000",
            Year = 2010,
            IdOwner = o2.IdOwner,
            Owner = o2
        };

        db.Properties.AddRange(p1, p2, p3, p4, p5);
        await db.SaveChangesAsync(ct);

        // --- IMAGES (usando el Id real en la ruta) ---
        db.PropertyImages.AddRange(
            new PropertyImage { IdProperty = p1.IdProperty, File = $"uploads/properties/{p1.IdProperty}/727634165.jpg", Enabled = true },
            new PropertyImage { IdProperty = p1.IdProperty, File = $"uploads/properties/{p1.IdProperty}/727634189.jpg", Enabled = true },
            new PropertyImage { IdProperty = p1.IdProperty, File = $"uploads/properties/{p1.IdProperty}/727634192.jpg", Enabled = true },
            new PropertyImage { IdProperty = p1.IdProperty, File = $"uploads/properties/{p1.IdProperty}/727634195.jpg", Enabled = true },
            new PropertyImage { IdProperty = p1.IdProperty, File = $"uploads/properties/{p1.IdProperty}/727634206.jpg", Enabled = true }
        );

        // --- TRACES (histórico precio) ---
        db.PropertyTraces.AddRange(
            new PropertyTrace { IdProperty = p1.IdProperty, DateSale = new DateOnly(2025, 1, 1), Name = "Price set", Value = 850_000m, Tax = 0m },
            new PropertyTrace { IdProperty = p2.IdProperty, DateSale = new DateOnly(2024, 11, 15), Name = "Price set", Value = 520_000m, Tax = 0m },
            new PropertyTrace { IdProperty = p3.IdProperty, DateSale = new DateOnly(2023, 6, 20), Name = "Price set", Value = 1_200_000m, Tax = 0m },
            new PropertyTrace { IdProperty = p4.IdProperty, DateSale = new DateOnly(2024, 8, 12), Name = "Price set", Value = 1_050_000m, Tax = 0m },
            new PropertyTrace { IdProperty = p5.IdProperty, DateSale = new DateOnly(2024, 4, 5), Name = "Price set", Value = 690_000m, Tax = 0m }
        );

        await db.SaveChangesAsync(ct);
    }
}

using System;

namespace RealEstate.Domain.Models;

public class PropertyTrace
{
    public int IdPropertyTrace { get; init; }
    public int IdProperty { get; set; }
    public DateOnly DateSale { get; set; }
    public string Name { get; set; } = null!;
    public decimal Value { get; set; }
    public decimal? Tax { get; set; }
}

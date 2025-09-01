using RealEstate.Application.Contracts.Owners;

namespace RealEstate.Application.Contracts.Properties;
public  class PropertySummaryDto
{
    public int IdProperty { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public decimal Price { get; set; }
    public short? Year { get; set; }
    public string Owner { get; set; }
}
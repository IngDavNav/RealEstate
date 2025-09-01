using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Owners;

using System.Collections.Generic;

namespace RealEstate.Application.Contracts.Properties;

public class PropertyDetailDto
{
    public int IdProperty { get; set; }
    public string Name { get; set; } = null!;
    public AddressDto? Address { get; set; } = null!;
    public decimal Price { get; set; }
    public string CodeInternal { get; set; }
    public short? Year { get; set; }
    public OwnerDto Owner { get; set; }

    public IReadOnlyList<PropertyImageDto> Images { get; set; } = new List<PropertyImageDto>();
    public IReadOnlyList<PropertyTraceDto> Traces { get; set; } = new List<PropertyTraceDto>();

}

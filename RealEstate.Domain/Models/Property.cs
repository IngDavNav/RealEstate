using System.Collections.Generic;

namespace RealEstate.Domain.Models;

public class Property
{
    public int IdProperty { get; init; }
    public string Name { get; set; }
    public PropertyAddress? Address { get; set; } //Value Object
    public decimal Price { get; set; }
    public string CodeInternal { get; set; }
    public short? Year { get; set; }
    public int IdOwner { get; set; }
    public Owner Owner { get; set; }

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<PropertyTrace> Traces { get; set; } = new List<PropertyTrace>();
}

public class PropertyAddress
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}";
}

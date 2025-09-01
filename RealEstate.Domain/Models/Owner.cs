using System;

namespace RealEstate.Domain.Models;

public class Owner
{
    public int IdOwner { get; init; }
    public string Name { get; set; }
    public OwnerAddress? Address { get; set; } //Value Object
    public string? Photo { get; set; }
    public DateOnly? Birthday { get; set; }
}

public class OwnerAddress
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}";
}

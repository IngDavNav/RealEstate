using RealEstate.Application.Contracts.Commons;

using System;

namespace RealEstate.Application.Contracts.Owners;

public class OwnerDto
{
    public int IdOwner { get; init; }
    public string Name { get; set; }
    public AddressDto? Address { get; set; }
    public string? Photo { get; set; }
    public DateOnly? Birthday { get; set; }
}

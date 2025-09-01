using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Properties;

namespace RealEstate.Application.UseCases.CreatePropertyBuilding;

public class CreatePropertyBuildingCommand : ICommand<PropertyDetailDto>
{
    public string Name { get; set; }
    public CreateAddressCommand? Address { get; set; }
    public decimal Price { get; set; }
    public string? CodeInternal { get; set; }
    public short? Year { get; set; }
    public int IdOwner { get; set; }
    public bool CreateInitialTrace { get; set; } = true;
    public string InitialTraceName { get; set; } = "Property created";
    public decimal InitialTax { get; set; } = 0;
}

public class CreateAddressCommand { 
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

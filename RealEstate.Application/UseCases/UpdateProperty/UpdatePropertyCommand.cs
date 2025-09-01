using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Commons;

namespace RealEstate.Application.UseCases.UpdateProperty;

public class UpdatePropertyCommand : ICommand<bool>
{
    public int IdProperty { get; set; }
    public string Name { get; set; }
    public AddressDto? Address { get; set; }
    public decimal Price { get; set; }
    public string? CodeInternal { get; set; }
    public short? Year { get; set; }
    public int IdOwner { get; set; }
}

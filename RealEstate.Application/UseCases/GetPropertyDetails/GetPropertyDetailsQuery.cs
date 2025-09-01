using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Properties;

namespace RealEstate.Application.UseCases.GetPropertyDetails;

public class GetPropertyDetailsQuery : IQuery<PropertyDetailDto?>
{
    public int PropertyId;
};
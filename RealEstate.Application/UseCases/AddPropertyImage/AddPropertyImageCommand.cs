using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Files;
using RealEstate.Application.Contracts.Properties;

namespace RealEstate.Application.UseCases.AddPropertyImage
{
    public class AddPropertyImageCommand : ICommand<PropertyImageDto>
    {
        public int IdProperty { get;set;}
        public ImageUpload Image { get;set;}
        public bool Enabled { get; set; } = true;
    }
}

using AutoMapper;

using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.CreatePropertyBuilding;
using RealEstate.Application.UseCases.UpdateProperty;
using RealEstate.Domain.Models;

using System.Text.Json.Serialization;

namespace RealEstate.Application.Profiles
{
    public class PropertyProfiles : Profile
    {
        public PropertyProfiles()
        {
            CreateMap<PropertyAddress, AddressDto>();
            CreateMap<AddressDto, PropertyAddress>();


            CreateMap<PropertyImage, PropertyImageDto>();
            CreateMap<PropertyTrace, PropertyTraceDto>();

            CreateMap<Property, PropertyDetailDto>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
            .ForMember(d => d.Owner, opt => opt.MapFrom(s => s.Owner))
            .ForMember(d => d.Images, opt => opt.MapFrom(s => s.Images))
            .ForMember(d => d.Traces, opt => opt.MapFrom(s => s.Traces));

            CreateMap<Property, PropertySummaryDto>()
               .ForMember(d => d.Owner, opt => opt.MapFrom(s => s.Owner.Name))
               .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address.ToString()));

            CreateMap<CreateAddressCommand, PropertyAddress>();

            CreateMap<UpdatePropertyCommand, Property>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
            .ForMember(d => d.Owner, opt => opt.Ignore())
            .ForMember(d => d.Images, opt => opt.Ignore())
            .ForMember(d => d.Traces, opt => opt.Ignore());

            CreateMap<CreatePropertyBuildingCommand, Property>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
            .ForMember(d => d.Owner, opt => opt.Ignore())
            .ForMember(d => d.Images, opt => opt.Ignore())
            .ForMember(d => d.Traces, opt => opt.Ignore());
        }
    }
}

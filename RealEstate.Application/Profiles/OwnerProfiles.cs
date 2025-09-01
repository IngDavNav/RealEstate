using AutoMapper;

using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Owners;
using RealEstate.Domain.Models;

namespace RealEstate.Application.Profiles;

public class OwnerProfiles : Profile
{
    public OwnerProfiles()
    {
        CreateMap<OwnerAddress, AddressDto>();

        CreateMap<Owner, OwnerDto>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address));
    }
}

using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;

namespace RealEstate.Application.UseCases.GetPropertyList
{
    public class GetPropertyListQuery : IQuery<PagedDtos<PropertySummaryDto>>
    {
        public AddressDto? Address { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public short? Year { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

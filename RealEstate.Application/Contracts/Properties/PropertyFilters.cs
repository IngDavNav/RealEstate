using RealEstate.Application.Contracts.Commons;

namespace RealEstate.Application.Contracts.Properties
{
    public class PropertyFilters
    {
        public AddressDto? Address { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public short? Year { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public bool HasImage { get; init; } = false;
    }
}

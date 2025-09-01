using System.Collections.Generic;

namespace RealEstate.Application.Contracts.Commons
{
    public class PagedDtos<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public required int Total { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
    }
}

using AutoMapper;

using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Domain.Models;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.UseCases.GetPropertyList
{
    public class GetPropertyListHandler : IQueryHandler<GetPropertyListQuery, PagedDtos<PropertySummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetPropertyListHandler> _logger;
        private readonly IMapper _mapper;

        public GetPropertyListHandler(IUnitOfWork unitOfWork, ILogger<GetPropertyListHandler> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<PagedDtos<PropertySummaryDto>> Handle(GetPropertyListQuery query, CancellationToken cancellationToken)
        {
            var filters = new PropertyFilters
            {
                Address = query.Address,
                MinPrice = query.MinPrice,
                MaxPrice = query.MaxPrice,
                Year = query.Year,
                Page = query.Page,
                PageSize = query.PageSize
            };

            var properties = await _unitOfWork.Properties.GetPropertyByFiltersAsync(filters, cancellationToken);
            _logger.LogInformation("List properties page={Page} size={Size} filters: {Addr}/{Min}/{Max}/{Year}",
                filters.Page, filters.PageSize, filters.Address, filters.MinPrice, filters.MaxPrice, filters.Year);

            var propertiesDto = new PagedDtos<PropertySummaryDto>
            {
                Total = properties.Total,
                Items = _mapper.Map<List<PropertySummaryDto>>(properties.Items),
                Page = properties.Page,
                PageSize = properties.PageSize
            };

            return propertiesDto;
        }
    }
}

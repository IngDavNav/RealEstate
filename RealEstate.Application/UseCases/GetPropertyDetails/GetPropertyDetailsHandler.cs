using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Files;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Domain.Models;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.UseCases.GetPropertyDetails;

public class GetPropertyDetailsHandler : IQueryHandler<GetPropertyDetailsQuery, PropertyDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPropertyDetailsHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IImageUrlBuilder _urlBuilder;
    private readonly IHttpContextAccessor _http;

    public GetPropertyDetailsHandler(IUnitOfWork unitOfWork, ILogger<GetPropertyDetailsHandler> logger, IMapper mapper, IImageUrlBuilder urlBuilder, IHttpContextAccessor http)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _urlBuilder = urlBuilder;
        _http = http;
    }


    public async Task<PropertyDetailDto> Handle(GetPropertyDetailsQuery query, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var property = await _unitOfWork.Properties.GetDetailAsync(query.PropertyId, cancellationToken);
        sw.Stop();

        if (property is null)
        {
            _logger.LogInformation($"GetPropertyDetail({query.PropertyId}) not found in {sw.ElapsedMilliseconds} ms");
            return null;
        }
        
        property.Images = property.Images?.OrderBy(i => i.IdPropertyImage).ToList() ?? new List<PropertyImage>();
        property.Traces = property.Traces?
            .OrderByDescending(t => t.DateSale)
            .ThenByDescending(t => t.IdPropertyTrace)
            .ToList() ?? new List<PropertyTrace>();
        var dto = _mapper.Map<PropertyDetailDto>(property);

        var req = _http.HttpContext!.Request;
        foreach (var img in dto.Images)
        {
            img.Url = _urlBuilder.ToPublicUrl("/" + img.File.TrimStart('/'), req);
        }

        _logger.LogInformation($"GetPropertyDetail({query.PropertyId}) in {sw.ElapsedMilliseconds} ms");

        return dto;
    }
}

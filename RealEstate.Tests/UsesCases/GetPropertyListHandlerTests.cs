using AutoMapper;

using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.GetPropertyList;
using RealEstate.Domain.Models;

using Xunit;

namespace RealEstate.Tests.UsesCases;

public class GetPropertyListHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<GetPropertyListHandler>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPropertyRepository> _propertyRepoMock = new();
    private readonly GetPropertyListHandler _handler;

    public GetPropertyListHandlerTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Properties).Returns(_propertyRepoMock.Object);
        _handler = new GetPropertyListHandler(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMappedPagedDtos()
    {
        // Arrange
        var query = new GetPropertyListQuery
        {
            Address = new AddressDto { Street = "Main", City = "Test", State = "TS", ZipCode = "12345" },
            MinPrice = 100,
            MaxPrice = 500,
            Year = 2020,
            Page = 2,
            PageSize = 10
        };

        var pagedProperties = new PagedDtos<Property>
        {
            Total = 2,
            Page = 2,
            PageSize = 10,
            Items = new List<Property>
            {
                new Property { IdProperty = 1, Name = "Prop1", Price = 200 },
                new Property { IdProperty = 2, Name = "Prop2", Price = 300 }
            }
        };

        var mappedDtos = new List<PropertySummaryDto>
        {
            new PropertySummaryDto { IdProperty = 1, Name = "Prop1", Price = 200 },
            new PropertySummaryDto { IdProperty = 2, Name = "Prop2", Price = 300 }
        };

        _propertyRepoMock.Setup(r => r.GetPropertyByFiltersAsync(It.IsAny<PropertyFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProperties);

        _mapperMock.Setup(m => m.Map<List<PropertySummaryDto>>(pagedProperties.Items))
            .Returns(mappedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Prop1", result.Items[0].Name);
        Assert.Equal("Prop2", result.Items[1].Name);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyPagedDtos()
    {
        // Arrange
        var query = new GetPropertyListQuery
        {
            Page = 1,
            PageSize = 5
        };

        var pagedProperties = new PagedDtos<Property>
        {
            Total = 0,
            Page = 1,
            PageSize = 5,
            Items = new List<Property>()
        };

        var mappedDtos = new List<PropertySummaryDto>();

        _propertyRepoMock.Setup(r => r.GetPropertyByFiltersAsync(It.IsAny<PropertyFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProperties);

        _mapperMock.Setup(m => m.Map<List<PropertySummaryDto>>(pagedProperties.Items))
            .Returns(mappedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Empty(result.Items);
    }
}
using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Contracts.Files;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.GetPropertyDetails;
using RealEstate.Domain.Models;

using Xunit;

namespace RealEstate.Tests.UsesCases;

public class GetPropertyDetailsHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<GetPropertyDetailsHandler>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IImageUrlBuilder> _urlBuilderMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<HttpRequest> _httpRequestMock = new();
    private readonly Mock<HttpContext> _httpContextMock = new();

    private readonly GetPropertyDetailsHandler _handler;

    public GetPropertyDetailsHandlerTests()
    {
        _httpContextMock.SetupGet(x => x.Request).Returns(_httpRequestMock.Object);
        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(_httpContextMock.Object);

        _handler = new GetPropertyDetailsHandler(
            _unitOfWorkMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _urlBuilderMock.Object,
            _httpContextAccessorMock.Object
        );
    }

    [Fact]
    public async Task Handle_PropertyNotFound_ReturnsNullAndLogs()
    {
        // Arrange
        var query = new GetPropertyDetailsQuery { PropertyId = 123 };
        _unitOfWorkMock.Setup(u => u.Properties.GetDetailAsync(123, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Property)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_PropertyFound_ImagesAndTracesAreOrdered_UrlsAreSet_LogsAndReturnsDto()
    {
        // Arrange
        var query = new GetPropertyDetailsQuery { PropertyId = 456 };
        var property = new Property
        {
            IdProperty = 456,
            Name = "Test Property",
            Images = new List<PropertyImage>
            {
                new PropertyImage { IdPropertyImage = 2, File = "img2.jpg", Enabled = true },
                new PropertyImage { IdPropertyImage = 1, File = "img1.jpg", Enabled = true }
            },
            Traces = new List<PropertyTrace>
            {
                new PropertyTrace { IdPropertyTrace = 10, DateSale = new DateOnly(2023, 1, 1), Name = "Sale1", Value = 100, Tax = 10 },
                new PropertyTrace { IdPropertyTrace = 11, DateSale = new DateOnly(2024, 1, 1), Name = "Sale2", Value = 200, Tax = 20 }
            }
        };

        var dto = new PropertyDetailDto
        {
            IdProperty = 456,
            Name = "Test Property",
            Images = new List<PropertyImageDto>
            {
                new PropertyImageDto { IdPropertyImage = 1, File = "img1.jpg" },
                new PropertyImageDto { IdPropertyImage = 2, File = "img2.jpg" }
            },
            Traces = new List<PropertyTraceDto>
            {
                new PropertyTraceDto { IdPropertyTrace = 11, DateSale = new DateOnly(2024, 1, 1), Name = "Sale2", Value = 200, Tax = 20 },
                new PropertyTraceDto { IdPropertyTrace = 10, DateSale = new DateOnly(2023, 1, 1), Name = "Sale1", Value = 100, Tax = 10 }
            }
        };

        _unitOfWorkMock.Setup(u => u.Properties.GetDetailAsync(456, It.IsAny<CancellationToken>()))
            .ReturnsAsync(property);

        _mapperMock.Setup(m => m.Map<PropertyDetailDto>(It.IsAny<Property>()))
            .Returns(dto);

        _urlBuilderMock.Setup(u => u.ToPublicUrl(It.IsAny<string>(), It.IsAny<HttpRequest>()))
            .Returns<string, HttpRequest>((path, req) => $"https://cdn.test/{path.TrimStart('/')}");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(456, result.IdProperty);
        Assert.Equal("Test Property", result.Name);

        // Images should be ordered by IdPropertyImage
        Assert.Equal(1, result.Images[0].IdPropertyImage);
        Assert.Equal(2, result.Images[1].IdPropertyImage);

        // Traces should be ordered by DateSale desc, then IdPropertyTrace desc
        Assert.Equal(11, result.Traces[0].IdPropertyTrace);
        Assert.Equal(10, result.Traces[1].IdPropertyTrace);

        // Urls should be set
        foreach (var img in result.Images)
        {
            Assert.StartsWith("https://cdn.test/", img.Url);
        }
    }

    [Fact]
    public async Task Handle_PropertyFound_NullImagesAndTraces_HandledGracefully()
    {
        // Arrange
        var query = new GetPropertyDetailsQuery { PropertyId = 789 };
        var property = new Property
        {
            IdProperty = 789,
            Name = "No Images or Traces",
            Images = null,
            Traces = null
        };

        var dto = new PropertyDetailDto
        {
            IdProperty = 789,
            Name = "No Images or Traces",
            Images = new List<PropertyImageDto>(),
            Traces = new List<PropertyTraceDto>()
        };

        _unitOfWorkMock.Setup(u => u.Properties.GetDetailAsync(789, It.IsAny<CancellationToken>()))
            .ReturnsAsync(property);

        _mapperMock.Setup(m => m.Map<PropertyDetailDto>(It.IsAny<Property>()))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Images);
        Assert.Empty(result.Traces);
    }
}
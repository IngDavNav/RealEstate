using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Api.Controllers;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Commons;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.AddPropertyImage;
using RealEstate.Application.UseCases.ChangePrice;
using RealEstate.Application.UseCases.CreatePropertyBuilding;
using RealEstate.Application.UseCases.GetPropertyDetails;
using RealEstate.Application.UseCases.GetPropertyList;
using RealEstate.Application.UseCases.UpdateProperty;

using Xunit;

namespace RealEstate.Tests.Controllers;

public class PropertiesControllerTests
{
    private readonly Mock<IQueryHandler<GetPropertyDetailsQuery, PropertyDetailDto>> _getDetailHandler = new();
    private readonly Mock<IQueryHandler<GetPropertyListQuery, PagedDtos<PropertySummaryDto>>> _getListHandler = new();
    private readonly Mock<ICommandHandler<ChangePriceCommand, bool>> _changePriceHandler = new();
    private readonly Mock<ICommandHandler<CreatePropertyBuildingCommand, PropertyDetailDto>> _createHandler = new();
    private readonly Mock<ICommandHandler<UpdatePropertyCommand, bool>> _updateHandler = new();
    private readonly Mock<ICommandHandler<AddPropertyImageCommand, PropertyImageDto>> _addImageHandler = new();
    private readonly Mock<ILogger<PropertiesController>> _logger = new();

    private PropertiesController CreateController() => new(
        _getDetailHandler.Object,
        _getListHandler.Object,
        _changePriceHandler.Object,
        _createHandler.Object,
        _updateHandler.Object,
        _addImageHandler.Object,
        _logger.Object
    );

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var dto = new PropertyDetailDto { IdProperty = 1, Name = "Test" };
        _getDetailHandler.Setup(h => h.Handle(It.IsAny<GetPropertyDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = CreateController();

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(dto, ok.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNull()
    {
        _getDetailHandler.Setup(h => h.Handle(It.IsAny<GetPropertyDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyDetailDto)null);

        var controller = CreateController();

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsOkWithPagedDtos()
    {
        var paged = new PagedDtos<PropertySummaryDto>
        {
            Items = new List<PropertySummaryDto> { new PropertySummaryDto { IdProperty = 1, Name = "Test" } },
            Total = 1,
            Page = 1,
            PageSize = 10
        };
        _getListHandler.Setup(h => h.Handle(It.IsAny<GetPropertyListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var controller = CreateController();

        var result = await controller.Get(new GetPropertyListQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(paged, ok.Value);
    }

    [Fact]
    public async Task ChangePrice_ReturnsBadRequest_WhenIdMismatch()
    {
        var controller = CreateController();
        var cmd = new ChangePriceCommand { IdProperty = 2, NewPrice = 100 };

        var result = await controller.ChangePrice(1, cmd, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Id in URL", bad.Value.ToString());
    }

    [Fact]
    public async Task ChangePrice_ReturnsBadRequest_WhenPriceInvalid()
    {
        var controller = CreateController();
        var cmd = new ChangePriceCommand { IdProperty = 1, NewPrice = 0 };

        var result = await controller.ChangePrice(1, cmd, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("NewPrice", bad.Value.ToString());
    }

    [Fact]
    public async Task ChangePrice_ReturnsNoContent_WhenOk()
    {
        _changePriceHandler.Setup(h => h.Handle(It.IsAny<ChangePriceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController();
        var cmd = new ChangePriceCommand { IdProperty = 1, NewPrice = 100 };

        var result = await controller.ChangePrice(1, cmd, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ChangePrice_ReturnsNotFound_WhenNotOk()
    {
        _changePriceHandler.Setup(h => h.Handle(It.IsAny<ChangePriceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController();
        var cmd = new ChangePriceCommand { IdProperty = 1, NewPrice = 100 };

        var result = await controller.ChangePrice(1, cmd, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenInvalid()
    {
        var controller = CreateController();
        var cmd = new CreatePropertyBuildingCommand { Name = "", Price = 0, IdOwner = 0 };

        var result = await controller.Create(cmd, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenOk()
    {
        var dto = new PropertyDetailDto { IdProperty = 1, Name = "Test" };
        _createHandler.Setup(h => h.Handle(It.IsAny<CreatePropertyBuildingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = CreateController();
        var cmd = new CreatePropertyBuildingCommand
        {
            Name = "Test",
            Address = new CreateAddressCommand(),
            Price = 100,
            IdOwner = 1
        };

        var result = await controller.Create(cmd, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(dto, created.Value);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenKeyNotFound()
    {
        _createHandler.Setup(h => h.Handle(It.IsAny<CreatePropertyBuildingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("not found"));

        var controller = CreateController();
        var cmd = new CreatePropertyBuildingCommand
        {
            Name = "Test",
            Address = new CreateAddressCommand(),
            Price = 100,
            IdOwner = 1
        };

        var result = await controller.Create(cmd, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains("not found", notFound.Value.ToString());
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var controller = CreateController();
        var cmd = new UpdatePropertyCommand { IdProperty = 2, Name = "Test", Price = 100, IdOwner = 1 };

        var result = await controller.Update(1, cmd, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Id in URL", bad.Value.ToString());
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenOk()
    {
        _updateHandler.Setup(h => h.Handle(It.IsAny<UpdatePropertyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController();
        var cmd = new UpdatePropertyCommand { IdProperty = 1, Name = "Test", Price = 100, IdOwner = 1 };

        var result = await controller.Update(1, cmd, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenNotOk()
    {
        _updateHandler.Setup(h => h.Handle(It.IsAny<UpdatePropertyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController();
        var cmd = new UpdatePropertyCommand { IdProperty = 1, Name = "Test", Price = 100, IdOwner = 1 };

        var result = await controller.Update(1, cmd, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddImageForm_ReturnsBadRequest_WhenNoImage()
    {
        var controller = CreateController();

        var result = await controller.AddImageForm(1, null, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddImageForm_ReturnsCreated_WhenOk()
    {
        var dto = new PropertyImageDto { IdProperty = 1, File = "img.jpg" };
        _addImageHandler.Setup(h => h.Handle(It.IsAny<AddPropertyImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = CreateController();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(10);
        fileMock.Setup(f => f.FileName).Returns("img.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[10]));
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((stream, token) =>
            {
                stream.Write(new byte[10], 0, 10);
                return Task.CompletedTask;
            });

        var result = await controller.AddImageForm(1, fileMock.Object, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(dto, created.Value);
    }
}

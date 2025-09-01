using AutoMapper;

using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Files;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Application.UseCases.CreatePropertyBuilding;
using RealEstate.Domain.Models;

using System.Data;

using Xunit;

namespace RealEstate.Tests.UsesCases;
public class CreatePropertyBuildingHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IImageStorage> _imageStorageMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ILogger<CreatePropertyBuildingHandler>> _loggerMock = new();
    private readonly Mock<IOwnerRepository> _ownerRepoMock = new();
    private readonly Mock<IPropertyRepository> _propertyRepoMock = new();
    private readonly CreatePropertyBuildingHandler _handler;

    public CreatePropertyBuildingHandlerTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Owners).Returns(_ownerRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Properties).Returns(_propertyRepoMock.Object);
        _handler = new CreatePropertyBuildingHandler(
            _unitOfWorkMock.Object,
            _imageStorageMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_OwnerDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new CreatePropertyBuildingCommand { IdOwner = 1 };
        _ownerRepoMock.Setup(r => r.Exists(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CreatesPropertyWithoutInitialTrace_Success()
    {
        // Arrange
        var command = new CreatePropertyBuildingCommand
        {
            IdOwner = 2,
            Name = "Test Property",
            CreateInitialTrace = false
        };
        var tx = Mock.Of<IDbTransaction>();
        var property = new Property { IdProperty = 10, Name = "Test Property" };
        var propertyDto = new PropertyDetailDto { IdProperty = 10, Name = "Test Property" };

        _ownerRepoMock.Setup(r => r.Exists(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _mapperMock.Setup(m => m.Map<Property>(command)).Returns(property);
        _propertyRepoMock.Setup(r => r.CreateAsync(property, It.IsAny<CancellationToken>())).ReturnsAsync(property);
        _unitOfWorkMock.Setup(u => u.CommitAsync(tx, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<PropertyDetailDto>(property)).Returns(propertyDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(10, result.IdProperty);
        Assert.Equal("Test Property", result.Name);
        Assert.Empty(property.Traces);
        _unitOfWorkMock.Verify(u => u.CommitAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CreatesPropertyWithInitialTrace_Success()
    {
        // Arrange
        var command = new CreatePropertyBuildingCommand
        {
            IdOwner = 3,
            Name = "Property With Trace",
            CreateInitialTrace = true,
            InitialTraceName = "Initial Sale",
            Price = 100000,
            InitialTax = 500
        };
        var tx = Mock.Of<IDbTransaction>();
        var property = new Property { IdProperty = 20, Name = "Property With Trace" };
        var propertyDto = new PropertyDetailDto { IdProperty = 20, Name = "Property With Trace" };

        _ownerRepoMock.Setup(r => r.Exists(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _mapperMock.Setup(m => m.Map<Property>(command)).Returns(property);
        _propertyRepoMock.Setup(r => r.CreateAsync(property, It.IsAny<CancellationToken>())).ReturnsAsync(property);
        _unitOfWorkMock.Setup(u => u.CommitAsync(tx, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mapperMock.Setup(m => m.Map<PropertyDetailDto>(property)).Returns(propertyDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(20, result.IdProperty);
        Assert.Equal("Property With Trace", result.Name);
        Assert.NotNull(property.Traces);
        Assert.Single(property.Traces);
        Assert.Equal("Initial Sale", property.Traces.First().Name);
        Assert.Equal(100000, property.Traces.First().Value);
        Assert.Equal(500, property.Traces.First().Tax);
        _unitOfWorkMock.Verify(u => u.CommitAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionDuringCreate_RollsBackAndThrows()
    {
        // Arrange
        var command = new CreatePropertyBuildingCommand
        {
            IdOwner = 4,
            Name = "Fail Property"
        };
        var tx = Mock.Of<IDbTransaction>();
        var property = new Property { IdProperty = 30, Name = "Fail Property" };

        _ownerRepoMock.Setup(r => r.Exists(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _mapperMock.Setup(m => m.Map<Property>(command)).Returns(property);
        _propertyRepoMock.Setup(r => r.CreateAsync(property, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
    }
}
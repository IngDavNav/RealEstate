using AutoMapper;

using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.UseCases.UpdateProperty;
using RealEstate.Domain.Models;

using System.Data;

using Xunit;

namespace RealEstate.Tests.UsesCases;

public class UpdatePropertyHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<UpdatePropertyHandler>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IOwnerRepository> _ownerRepoMock = new();
    private readonly Mock<IPropertyRepository> _propertyRepoMock = new();
    private readonly UpdatePropertyHandler _handler;

    public UpdatePropertyHandlerTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Owners).Returns(_ownerRepoMock.Object);
        _unitOfWorkMock.SetupGet(u => u.Properties).Returns(_propertyRepoMock.Object);
        _handler = new UpdatePropertyHandler(_unitOfWorkMock.Object, _loggerMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_OwnerDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new UpdatePropertyCommand { IdOwner = 1, IdProperty = 10 };
        _ownerRepoMock.Setup(r => r.Exists(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SuccessfulUpdate_CommitsAndReturnsTrue()
    {
        // Arrange
        var command = new UpdatePropertyCommand
        {
            IdOwner = 2,
            IdProperty = 20,
            Name = "Updated",
            Price = 1000
        };
        var tx = Mock.Of<IDbTransaction>();
        var property = new Property { IdProperty = 20, Name = "Updated", Price = 1000 };

        _ownerRepoMock.Setup(r => r.Exists(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _mapperMock.Setup(m => m.Map<Property>(command)).Returns(property);
        _propertyRepoMock.Setup(r => r.UpdateAsync(property, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.CommitAsync(tx, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _unitOfWorkMock.Verify(u => u.CommitAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionDuringUpdate_RollsBackAndThrows()
    {
        // Arrange
        var command = new UpdatePropertyCommand
        {
            IdOwner = 3,
            IdProperty = 30,
            Name = "ShouldFail"
        };
        var tx = Mock.Of<IDbTransaction>();
        var property = new Property { IdProperty = 30, Name = "ShouldFail" };

        _ownerRepoMock.Setup(r => r.Exists(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _mapperMock.Setup(m => m.Map<Property>(command)).Returns(property);
        _propertyRepoMock.Setup(r => r.UpdateAsync(property, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
    }
}
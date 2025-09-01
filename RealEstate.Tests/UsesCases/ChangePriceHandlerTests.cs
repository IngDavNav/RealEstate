using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.UseCases.ChangePrice;

using System.Data;

using Xunit;

namespace RealEstate.Tests.UsesCases;

public class ChangePriceHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<ChangePriceHandler>> _loggerMock = new();
    private readonly Mock<IPropertyRepository> _propertyRepoMock = new();
    private readonly ChangePriceHandler _handler;

    public ChangePriceHandlerTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Properties).Returns(_propertyRepoMock.Object);
        _handler = new ChangePriceHandler(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewPriceLessThanOrEqualToZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var command = new ChangePriceCommand
        {
            IdProperty = 1,
            NewPrice = 0,
            DateOfChange = new DateOnly(2024, 1, 1)
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ChangePriceAsyncReturnsZero_RollsBackAndReturnsFalse()
    {
        // Arrange
        var command = new ChangePriceCommand
        {
            IdProperty = 2,
            NewPrice = 1000,
            DateOfChange = new DateOnly(2024, 1, 1)
        };
        var tx = Mock.Of<IDbTransaction>();

        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _propertyRepoMock.Setup(r => r.ChangePriceAsync(2, 1000, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<IDbTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ChangePriceAsyncReturnsOne_CommitsAndReturnsTrue()
    {
        // Arrange
        var command = new ChangePriceCommand
        {
            IdProperty = 3,
            NewPrice = 2000,
            DateOfChange = new DateOnly(2024, 1, 1)
        };
        var tx = Mock.Of<IDbTransaction>();

        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _propertyRepoMock.Setup(r => r.ChangePriceAsync(3, 2000, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.CommitAsync(tx, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        _unitOfWorkMock.Verify(u => u.CommitAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Never);
    }

    [Fact]
    public async Task Handle_ExceptionDuringChangePrice_RollsBackAndThrows()
    {
        // Arrange
        var command = new ChangePriceCommand
        {
            IdProperty = 4,
            NewPrice = 3000,
            DateOfChange = new DateOnly(2024, 1, 1)
        };
        var tx = Mock.Of<IDbTransaction>();

        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _propertyRepoMock.Setup(r => r.ChangePriceAsync(4, 3000, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
    }
}
using Microsoft.Extensions.Logging;

using Moq;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Files;
using RealEstate.Application.Abstractions.Repositories;
using RealEstate.Application.Contracts.Files;
using RealEstate.Application.UseCases.AddPropertyImage;
using RealEstate.Domain.Models;

using System.Data;

using Xunit;

namespace RealEstate.Tests.UsesCases;

public class AddPropertyImageHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IImageStorage> _storageMock = new();
    private readonly Mock<ILogger<AddPropertyImageHandler>> _loggerMock = new();
    private readonly Mock<IPropertyRepository> _propertyRepoMock = new();
    private readonly AddPropertyImageHandler _handler;

    public AddPropertyImageHandlerTests()
    {
        _unitOfWorkMock.SetupGet(u => u.Properties).Returns(_propertyRepoMock.Object);
        _handler = new AddPropertyImageHandler(_unitOfWorkMock.Object, _storageMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_PropertyDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var command = new AddPropertyImageCommand
        {
            IdProperty = 1,
            Image = new ImageUpload { Content = new byte[] { 1, 2, 3 }, FileName = "img.jpg", ContentType = "image/jpeg" },
            Enabled = true
        };
        _propertyRepoMock.Setup(r => r.Exists(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SuccessfulUploadAndAddImage_CommitsAndReturnsDto()
    {
        // Arrange
        var command = new AddPropertyImageCommand
        {
            IdProperty = 2,
            Image = new ImageUpload { Content = new byte[] { 1, 2, 3 }, FileName = "img.jpg", ContentType = "image/jpeg" },
            Enabled = true
        };
        var tx = Mock.Of<IDbTransaction>();
        _propertyRepoMock.Setup(r => r.Exists(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "img.jpg", "image/jpeg", "properties/2", It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/path/img.jpg");
        _propertyRepoMock.Setup(r => r.AddImageAsync(It.Is<PropertyImage>(img =>
            img.IdProperty == 2 && img.File == "stored/path/img.jpg" && img.Enabled), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);
        _unitOfWorkMock.Setup(u => u.CommitAsync(tx, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.IdPropertyImage);
        Assert.Equal(2, result.IdProperty);
        Assert.Equal("stored/path/img.jpg", result.File);
        Assert.True(result.Enabled);
        _unitOfWorkMock.Verify(u => u.CommitAsync(tx, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionDuringUpload_RollsBackAndThrows()
    {
        // Arrange
        var command = new AddPropertyImageCommand
        {
            IdProperty = 3,
            Image = new ImageUpload { Content = new byte[] { 1, 2, 3 }, FileName = "img.jpg", ContentType = "image/jpeg" },
            Enabled = false
        };
        var tx = Mock.Of<IDbTransaction>();
        _propertyRepoMock.Setup(r => r.Exists(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "img.jpg", "image/jpeg", "properties/3", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Upload failed"));
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionAfterUpload_DeletesFileOnRollback()
    {
        // Arrange
        var command = new AddPropertyImageCommand
        {
            IdProperty = 4,
            Image = new ImageUpload { Content = new byte[] { 1, 2, 3 }, FileName = "img.jpg", ContentType = "image/jpeg" },
            Enabled = true
        };
        var tx = Mock.Of<IDbTransaction>();
        _propertyRepoMock.Setup(r => r.Exists(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "img.jpg", "image/jpeg", "properties/4", It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/path/img2.jpg");
        _propertyRepoMock.Setup(r => r.AddImageAsync(It.IsAny<PropertyImage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);
        _storageMock.Setup(s => s.DeleteAsync("stored/path/img2.jpg", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
        _storageMock.Verify(s => s.DeleteAsync("stored/path/img2.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExceptionAfterUpload_AndDeleteAlsoFails_ThrowsOriginal()
    {
        // Arrange
        var command = new AddPropertyImageCommand
        {
            IdProperty = 5,
            Image = new ImageUpload { Content = new byte[] { 1, 2, 3 }, FileName = "img.jpg", ContentType = "image/jpeg" },
            Enabled = false
        };
        var tx = Mock.Of<IDbTransaction>();
        _propertyRepoMock.Setup(r => r.Exists(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "img.jpg", "image/jpeg", "properties/5", It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored/path/img3.jpg");
        _propertyRepoMock.Setup(r => r.AddImageAsync(It.IsAny<PropertyImage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));
        _unitOfWorkMock.Setup(u => u.RollbackAsync(tx)).Returns(Task.CompletedTask);
        _storageMock.Setup(s => s.DeleteAsync("stored/path/img3.jpg", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _handler.Handle(command, CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.RollbackAsync(tx), Times.Once);
        _storageMock.Verify(s => s.DeleteAsync("stored/path/img3.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }
}
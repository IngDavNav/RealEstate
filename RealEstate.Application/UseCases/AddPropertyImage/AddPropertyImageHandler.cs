using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Files;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Domain.Models;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.UseCases.AddPropertyImage
{
    public class AddPropertyImageHandler : ICommandHandler<AddPropertyImageCommand, PropertyImageDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageStorage _storage;
        private readonly ILogger<AddPropertyImageHandler> _logger;

        public AddPropertyImageHandler(IUnitOfWork unitOfWork, IImageStorage storage, ILogger<AddPropertyImageHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _storage = storage;
            _logger = logger;
        }


        public async Task<PropertyImageDto> Handle(AddPropertyImageCommand command, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.Properties.Exists(command.IdProperty, cancellationToken);
            if (!exists) throw new KeyNotFoundException($"Property {command.IdProperty} not found");

            var tx = await _unitOfWork.BeginAsync(cancellationToken);
            string? stored = null;
            try
            {
                using var ms = new MemoryStream(command.Image.Content);
                stored = await _storage.UploadAsync(ms, command.Image.FileName, command.Image.ContentType, $"properties/{command.IdProperty}", cancellationToken);

                var entity = new PropertyImage { IdProperty = command.IdProperty, File = stored, Enabled = command.Enabled };
                var imageId = await _unitOfWork.Properties.AddImageAsync(entity, cancellationToken);

                await _unitOfWork.CommitAsync(tx, cancellationToken);
                _logger.LogInformation($"Image {imageId} added to Property {command.IdProperty}");

                return new PropertyImageDto { IdPropertyImage = imageId, IdProperty = command.IdProperty, File = stored, Enabled = command.Enabled };
            }
            catch
            {
                await _unitOfWork.RollbackAsync(tx);
                if (stored is not null)
                    try
                    {
                        await _storage.DeleteAsync(stored, cancellationToken);
                    }
                    catch
                    {
                        throw;
                    }
                throw;
            }
        }
    }
}

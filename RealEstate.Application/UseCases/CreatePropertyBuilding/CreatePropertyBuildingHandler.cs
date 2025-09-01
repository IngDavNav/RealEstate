using AutoMapper;

using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Files;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Application.Contracts.Properties;
using RealEstate.Domain.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RealEstate.Application.UseCases.CreatePropertyBuilding;

public class CreatePropertyBuildingHandler : ICommandHandler<CreatePropertyBuildingCommand, PropertyDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorage _imageStorage;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePropertyBuildingHandler> _logger;

    public CreatePropertyBuildingHandler(
        IUnitOfWork unitOfWork,
        IImageStorage imageStorage,
        IMapper mapper,
        ILogger<CreatePropertyBuildingHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _imageStorage = imageStorage;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<PropertyDetailDto> Handle(CreatePropertyBuildingCommand command, CancellationToken cancellationToken)
    {
        var ownerExists = await _unitOfWork.Owners.Exists(command.IdOwner, cancellationToken);
        if (!ownerExists)
        {
            throw new KeyNotFoundException($"Owner {command.IdOwner} not found");
        }

        var tx = await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            var newProperty = _mapper.Map<Property>(command);
            if (command.CreateInitialTrace)
            {
                newProperty.Traces = new List<PropertyTrace>
                {
                    new PropertyTrace
                    {
                    IdProperty = newProperty.IdProperty,
                    DateSale = DateOnly.FromDateTime(DateTime.UtcNow),
                    Name = command.InitialTraceName,
                    Value = command.Price,
                    Tax = command.InitialTax
                    }
                };
            }
            newProperty = await _unitOfWork.Properties.CreateAsync(newProperty, cancellationToken);
            await _unitOfWork.CommitAsync(tx, cancellationToken);
            _logger.LogInformation($"Property {command.Name} created with Id {newProperty.IdProperty}");
            return _mapper.Map<PropertyDetailDto>(newProperty);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(tx);
            throw;
        }
    }
}

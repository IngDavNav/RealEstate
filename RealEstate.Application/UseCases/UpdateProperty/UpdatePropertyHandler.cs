using AutoMapper;

using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Domain.Models;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.UseCases.UpdateProperty
{
    public sealed class UpdatePropertyHandler : ICommandHandler<UpdatePropertyCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePropertyHandler> _logger;
        private readonly IMapper _mapper;

        public UpdatePropertyHandler(IUnitOfWork unitOfWork, ILogger<UpdatePropertyHandler> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdatePropertyCommand command, CancellationToken cancellationToken)
        {
            if (!await _unitOfWork.Owners.Exists(command.IdOwner, cancellationToken))
                throw new KeyNotFoundException($"Owner {command.IdOwner} not found");

            var tx = await _unitOfWork.BeginAsync(cancellationToken);
            try
            {
                var entity = _mapper.Map<Property>(command);

                await _unitOfWork.Properties.UpdateAsync(entity, cancellationToken);
                await _unitOfWork.CommitAsync(tx, cancellationToken);

                _logger.LogInformation("Property {Id} updated", command.IdProperty);
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync(tx);
                throw;
            }
        }
    }
}

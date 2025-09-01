using Microsoft.Extensions.Logging;

using RealEstate.Application.Abstractions;
using RealEstate.Application.Abstractions.Messaging;
using RealEstate.Domain.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RealEstate.Application.UseCases.ChangePrice
{
    public class ChangePriceHandler : ICommandHandler<ChangePriceCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ChangePriceHandler> _logger;

        public ChangePriceHandler(IUnitOfWork unitOfWork, ILogger<ChangePriceHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        public async Task<bool> Handle(ChangePriceCommand command, CancellationToken cancellationToken)
        {
            if (command.NewPrice <= 0) throw new ArgumentOutOfRangeException(nameof(command.NewPrice));

            var tx = await _unitOfWork.BeginAsync(cancellationToken);
            try
            {
                var updated = await _unitOfWork.Properties.ChangePriceAsync(command.IdProperty, command.NewPrice, cancellationToken);
                if (updated == 0)
                {
                    await _unitOfWork.RollbackAsync(tx);
                    return false;
                }

                await _unitOfWork.CommitAsync(tx);
                _logger.LogInformation("Price changed for property {Id} -> {Price}", command.IdProperty, command.NewPrice);
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

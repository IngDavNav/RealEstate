using RealEstate.Application.Abstractions.Messaging;

using System;

namespace RealEstate.Application.UseCases.ChangePrice
{
    public class ChangePriceCommand : ICommand<bool>
    {
        public int IdProperty { get; set; }
        public decimal NewPrice { get; set; }
        public DateOnly DateOfChange { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    }
}

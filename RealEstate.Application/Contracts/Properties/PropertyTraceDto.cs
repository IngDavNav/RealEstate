using System;

namespace RealEstate.Application.Contracts.Properties
{
    public class PropertyTraceDto
    {
        public int IdPropertyTrace { get; set; }
        public int IdProperty { get; set; }
        public DateOnly? DateSale { get; set; }
        public string Name { get; set; } = null!;
        public decimal Value { get; set; }
        public decimal Tax { get; set; }
    }
}

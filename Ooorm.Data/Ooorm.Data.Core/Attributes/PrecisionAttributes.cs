using System;

namespace Ooorm.Data.Core.Attributes
{
    public class DecimalPrecisionAttribute : Attribute
    {
        public readonly int Total;
        public readonly int Fractional;

        public DecimalPrecisionAttribute(int total, int fractional)
        {
            Total = total;
            Fractional = fractional;
        }
    }

    public class CurrencyAttribute : DecimalPrecisionAttribute
    {
        public CurrencyAttribute() : base(16, 2) { }
    }
}

using System;

namespace Ooorm.Data.Attributes
{
    public class FixedLengthAttribute : Attribute
    {
        public readonly int Length;

        public FixedLengthAttribute(int value) => Length = value;
    }

    public class MaxLengthAttribute : Attribute
    {
        public readonly int Length;

        public MaxLengthAttribute(int value) => Length = value;
    }
}

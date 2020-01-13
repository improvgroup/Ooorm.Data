using System;

namespace Ooorm.Data.Matching
{
    public abstract class ExpressionDecorator<TSelf, TValue> where TSelf : ExpressionDecorator<TSelf, TValue>
    {
        public static implicit operator TValue(ExpressionDecorator<TSelf, TValue> _) => throw new InvalidOperationException();

        public static explicit operator ExpressionDecorator<TSelf, TValue>(TValue _) => throw new InvalidOperationException();
    }

    public class Like : ExpressionDecorator<Like, string> { }

    public class GreaterThan<T> : ExpressionDecorator<GreaterThan<T>, T> where T : IComparable<T> { }

    public class LessThan<T> : ExpressionDecorator<LessThan<T>, T> where T : IComparable<T> { }

    public class NotGreaterThan<T> : ExpressionDecorator<NotGreaterThan<T>, T> where T : IComparable<T> { }

    public class NotLessThan<T> : ExpressionDecorator<NotLessThan<T>, T> where T : IComparable<T> { }

    public class Not<T> : ExpressionDecorator<Not<T>, T> { }
}

using FluentAssertions;
using Ooorm.Data.Matching;
using System;
using System.Linq.Expressions;
using Xunit;

namespace Ooorm.Data.Tests
{
    public class MatchExpressions_Should
    {
        public class Widget : DbItem<Widget, int>
        {
            public string Value { get; set; }
            public int Count { get; set; }
        }

        /// <summary>
        /// Confirm expression tree is built as expected
        /// Depends on ToString behavior of Expression<T> so may be fragile to BCL changes
        /// </summary>
        [Fact]
        public void SupportGtLtLike()
        {
            Expression<Func<Widget>> match = () => new Widget { Value = (Like) "%a" };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Value = Convert(Convert(Convert(\"%a\", ExpressionDecorator`2), Like), String)}");

            match = () => new Widget { Value = (GreaterThan<string>) "a" };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Value = Convert(Convert(Convert(\"a\", ExpressionDecorator`2), GreaterThan`1), String)}");

            match = () => new Widget { Count = (GreaterThan<int>) 2 };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Count = Convert(Convert(Convert(2, ExpressionDecorator`2), GreaterThan`1), Int32)}");

            match = () => new Widget { Count = (NotGreaterThan<int>) 2 };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Count = Convert(Convert(Convert(2, ExpressionDecorator`2), NotGreaterThan`1), Int32)}");

            match = () => new Widget { Value = (LessThan<string>)"a" };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Value = Convert(Convert(Convert(\"a\", ExpressionDecorator`2), LessThan`1), String)}");

            match = () => new Widget { Count = (LessThan<int>)2 };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Count = Convert(Convert(Convert(2, ExpressionDecorator`2), LessThan`1), Int32)}");

            match = () => new Widget { Count = (NotLessThan<int>)2 };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Count = Convert(Convert(Convert(2, ExpressionDecorator`2), NotLessThan`1), Int32)}");

            match = () => new Widget { Value = (Not<string>) "a" };
            match.ToString().Should().BeEquivalentTo("() => new Widget() {Value = Convert(Convert(Convert(\"a\", ExpressionDecorator`2), Not`1), String)}");

            match = () => new Widget { Value = (Like)"%a", Count = (Not<int>) 3 };
            match.ToString()
                 .Should()
                 .BeEquivalentTo(
                "() => new Widget() {Value = Convert(Convert(Convert(\"%a\", ExpressionDecorator`2), Like), String), Count = Convert(Convert(Convert(3, ExpressionDecorator`2), Not`1), Int32)}");
        }
    }
}

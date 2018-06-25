using Ooorm.Data.QueryProviders;
using Xunit;
using FluentAssertions;

namespace Ooorm.Data.SqlServer.Tests
{
    public class WhereClause_Should
    {
        private class DbModel : IDbItem
        {
            public int ID { get; set; }
            public string Key { get; set; }
            public int Value { get; set; }
        }

        private SqlServerQueryProvider<DbModel> provider => new SqlServerQueryProvider<DbModel>();

        private static readonly string ID = $"[{nameof(DbModel.ID)}]";
        private static readonly string KEY = $"[{nameof(DbModel.Key)}]";
        private static readonly string Value = $"[{nameof(DbModel.Value)}]";

        [Fact]
        public void SupportTrue()
        {
            string clause = provider.WhereClause(m => true);

            clause.Should().Be("WHERE 1=1");
        }

        [Fact]
        public void SupportFalse()
        {
            string clause = provider.WhereClause(m => false);

            clause.Should().Be("WHERE 1=0");
        }

        [Fact]
        public void SupportNullChecking()
        {
            string clause = provider.WhereClause(m => m.Key != null);

            clause.Should().Be($"WHERE ({KEY} IS NOT NULL)");
        }

        [Fact]
        public void SupportNotNullChecking()
        {
            string clause = provider.WhereClause(m => m.Key == null);

            clause.Should().Be($"WHERE ({KEY} IS NULL)");
        }

        [Fact]
        public void SupportFieldComparisonToParameterizedValue()
        {
            string clause = provider.WhereClause<DbModel>((m, p) => m.Key == p.Key);

            clause.Should().Be($"WHERE ({KEY} == @{nameof(DbModel.Key)})");
        }

        [Fact]
        public void SupportCompoundStatements_WithAnd()
        {
            string clause = provider.WhereClause(m => m.Key == null && m.Value > 2);

            clause.Should().Be($"WHERE (({KEY} IS NULL) AND ({Value} > 2))");
        }

        [Fact]
        public void SupportCompoundStatements_WithOr()
        {
            string clause = provider.WhereClause(m => m.Key == null || m.Value > 2);

            clause.Should().Be($"WHERE (({KEY} IS NULL) OR ({Value} > 2))");
        }

        [Fact]
        public void SupportNestedExpressions()
        {

        }
    }
}

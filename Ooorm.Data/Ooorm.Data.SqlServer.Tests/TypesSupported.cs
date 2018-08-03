using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ooorm.Data.SqlServer.Tests
{
    public class TypesSupported
    {
        public class NullablesItem : IDbItem
        {
            public int ID { get; set; }
            public int? Value { get; set; }
            public DateTimeOffset? Time { get; set; }
        }

        [Fact]
        public async Task NullableOfDbType()
        {
            await TestFixture.WithTempDb(async db =>
            {
                await typeof(NullablesItem).CreateTableIn(db);

                var item1 = await new NullablesItem { }.WriteTo(db);
                var item2 = await new NullablesItem { Value = 1 }.WriteTo(db);
                var item3 = await new NullablesItem { Time = DateTimeOffset.Now }.WriteTo(db);
                var item4 = await new NullablesItem { Value = 2, Time = DateTimeOffset.Now }.WriteTo(db);

                var itemsWithValue = await db.Read<NullablesItem>(r => r.Value != null);
                itemsWithValue.Should().ContainSingle(i => i.Value == 1);
                itemsWithValue.Should().ContainSingle(i => i.Value == 2);

                (await new NullablesItem { Time = item3.Time }.ReadMatchingFrom(db)).Single().ID.Should().Be(item3.ID);
            });
        }
    }
}

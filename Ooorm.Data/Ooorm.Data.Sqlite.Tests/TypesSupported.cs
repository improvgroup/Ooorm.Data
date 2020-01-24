using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ooorm.Data.Sqlite.Tests
{
    public class TypesSupported
    {
        public class NullablesItem : DbItem<NullablesItem, int>
        {
            public int? Value { get; set; }
            public DateTimeOffset? Time { get; set; }
            public string Text { get; set; }
        }

        [Fact]
        public async Task NullableOfDbType()
        {
            using var temp = new TestFixture.TempSqliteDb();

            await temp.db.CreateTable<NullablesItem, int>();

            var item1 = await new NullablesItem { }.WriteTo(temp.db);
            var item2 = await new NullablesItem { Value = 1 }.WriteTo(temp.db);

            var time = DateTimeOffset.Now;
            var item3 = await new NullablesItem { Time = time }.WriteTo(temp.db);
            var item4 = await new NullablesItem { Value = 2, Time = time }.WriteTo(temp.db);
            var item5 = await new NullablesItem { Text = "Content" }.WriteTo(temp.db);

            var itemsWithValue = await temp.db.Read<NullablesItem, int>(r => r.Value != null);
            itemsWithValue.Should().ContainSingle(i => i.Value == 1);
            itemsWithValue.Should().ContainSingle(i => i.Value == 2);

            var itemsWithoutValue = await temp.db.Read<NullablesItem, int>(r => r.Value == null);
            itemsWithoutValue.Should().ContainSingle(i => i.Time == time);
            itemsWithoutValue.Should().ContainSingle(i => i.Text == "Content");

            var results = await NullablesItem.ReadMatching(() => new NullablesItem { Text = item5.Text }).From(temp.db);
            results.Single().ID.Should().Be(item5.ID);
        }
    }
}

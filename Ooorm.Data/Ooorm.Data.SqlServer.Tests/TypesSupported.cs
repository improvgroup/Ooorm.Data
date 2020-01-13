using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ooorm.Data.SqlServer.Tests
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
        public Task NullableOfDbType() => TestFixture.WithTempDb(async db =>
        {
            await NullablesItem.CreateTable(db);

            var item1 = await new NullablesItem { }.WriteTo(db);
            var item2 = await new NullablesItem { Value = 1 }.WriteTo(db);

            var time = DateTimeOffset.Now;
            var item3 = await new NullablesItem { Time = time }.WriteTo(db);
            var item4 = await new NullablesItem { Value = 2, Time = time }.WriteTo(db);
            var item5 = await new NullablesItem { Text = "Content" }.WriteTo(db);

            var itemsWithValue = await NullablesItem.Read(r => r.Value != null).From(db);
            itemsWithValue.Should().ContainSingle(i => i.Value == 1);
            itemsWithValue.Should().ContainSingle(i => i.Value == 2);

            var itemsWithoutValue = await NullablesItem.Read(r => r.Value == null).From(db);
            itemsWithoutValue.Should().ContainSingle(i => i.Time == time);
            itemsWithoutValue.Should().ContainSingle(i => i.Text == "Content");

            (await new NullablesItem { Text = item5.Text }.ReadMatchingFrom(db)).Single().ID.Should().Be(item5.ID);
        });        
    }
}

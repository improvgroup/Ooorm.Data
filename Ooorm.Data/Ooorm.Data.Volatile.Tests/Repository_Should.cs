using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ooorm.Data.Volatile.Tests
{
    public class Repository_Should
    {
        public class Item : IDbItem
        {
            public int ID { get; set; }
            public string Value { get; set; }

            public override string ToString() => $"{ID} {Value}";
        }

        [Fact]
        public async Task CreateReadUpdateDelete()
        {
            var repo = new VolatileRepository<Item>(() => null);

            var items = Enumerable.Range(0, 1000).Select(i => new Item { Value = Convert.ToString(i, 2) }).ToArray();

            await repo.Write(items);

            var results = await repo.Read();

            results.Should().BeEquivalentTo(items);

            await repo.Delete(r => r.Value.Contains("111"));

            results = await repo.Read();

            results.Should().BeEquivalentTo(items.Where(i => !i.Value.Contains("111")).ToArray());

            await repo.Update(
                results
                    .Where(i => i.Value.Contains("000"))
                    .Select(i => { i.Value = "Hello"; return i; })
                    .ToArray());

            results = await repo.Read();

            var expected =
                items
                    .Where(i => !i.Value.Contains("111"))
                    .Select(i => { if (i.Value.Contains("000")) { i.Value = "Hello"; } return i; })
                    .ToArray();

            results.Should().BeEquivalentTo(expected);
        }
    }
}

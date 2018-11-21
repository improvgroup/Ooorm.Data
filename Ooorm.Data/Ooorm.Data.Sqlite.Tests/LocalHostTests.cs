using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System;

namespace Ooorm.Data.SqlServer.Tests
{
    public class LocalHostTests
    {
        public class Doodad : IDbItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public DbRef<Widget> PrimaryWidgetId { get; set; }
        }

        public class Widget : IDbItem
        {
            public int ID { get; set; }
            public int Value { get; set; }
        }

        public class WidgetDoodad : IDbItem
        {
            public int ID { get; set; }
            public DbVal<Widget> WidgetId { get; set; }
            public DbVal<Doodad> DoodadId { get; set; }
            public bool Active { get; set; }
        }

        [Fact]
        public async Task CreateSimpleTable()
        {
            await TestFixture.WithTempDb(async db =>
            {
                await db.CreateTable<Widget>();
            });
        }

        [Fact]
        public async Task CrudTest()
        {
            await TestFixture.WithTempDb(async db =>
            {
                await db.CreateTables(typeof(Widget), typeof(WidgetDoodad), typeof(Doodad));

                var widget1 = new Widget { Value = 1 };
                var widget2 = new Widget { Value = 2 };
                var widget3 = new Widget { Value = 4 };

                var count = await db.Write(widget1, widget2, widget3);

                widget1.ID.Should().BeGreaterThan(0);
                widget2.ID.Should().BeGreaterThan(0);
                widget3.ID.Should().BeGreaterThan(0);
                widget1.ID.Should().NotBe(widget2.ID);
                widget2.ID.Should().NotBe(widget3.ID);
                count.Should().Be(3);

                var widgets = await db.Read<Widget>();
                foreach (var ab in widgets.Zip(new Widget[] { widget1, widget2, widget3 }, (a, b) => (a, b)))
                    ab.a.Should().BeEquivalentTo(ab.b);

                (await db.Read<Widget>(widget2.ID)).Should().BeEquivalentTo(widget2);

                var doodad1 = new Doodad
                {
                    Name = "First",
                    PrimaryWidgetId = widget1.In(db)
                };

                var doodad2 = new Doodad
                {
                    Name = "Second",
                    PrimaryWidgetId = widget3.In(db)
                };

                await db.Write(doodad1, doodad2);

                await db.Write(
                    new WidgetDoodad
                    {
                        DoodadId = doodad1.In(db),
                        WidgetId = widget1.In(db),
                        Active = true,
                    },
                    new WidgetDoodad
                    {
                        DoodadId = doodad2.In(db),
                        WidgetId = widget2.In(db),
                        Active = true,
                    },
                    new WidgetDoodad
                    {
                        DoodadId = doodad1.In(db),
                        WidgetId = widget2.In(db),
                        Active = true,
                    });

                var linksToWidget2 = (await db.Read<WidgetDoodad, int?>((r, p) => r.WidgetId == p, widget2.ID)).ToList();
                linksToWidget2.Count.Should().Be(2);

                var first = await db.Dereference(linksToWidget2.First().DoodadId);
                var last = await db.Dereference(linksToWidget2.Last().DoodadId);

                if (first.ID == doodad1.ID)
                    last.ID.Should().Be(doodad2.ID);
                else if (first.ID == doodad2.ID)
                    last.ID.Should().Be(doodad1.ID);
                else
                    false.Should().BeTrue();
            });
        }

        [Fact]
        public async Task ItemExtensionsTest()
        {
            await TestFixture.WithTempDb(async db =>
            {
                await typeof(Widget).CreateTableIn(db);
                await typeof(WidgetDoodad).CreateTableIn(db);
                await typeof(Doodad).CreateTableIn(db);

                var w1 = await new Widget { Value = 100 }.WriteTo(db);
                var w2 = await new Widget { Value = 200 }.WriteTo(db);

                await new Doodad { Name = "A", PrimaryWidgetId = w1.In(db) }.WriteTo(db);
                await new Doodad { Name = "B", PrimaryWidgetId = w2.In(db) }.WriteTo(db);
                await new Doodad { Name = "C", PrimaryWidgetId = w2.In(db) }.WriteTo(db);

                var a = (await new Doodad { Name = "A" }.ReadMatchingFrom(db)).Single();

                a.Name.Should().Be("A");
                var w1froma = await a.PrimaryWidgetId.Get();
                w1froma.Value.Should().Be(100);

                var all = await new Doodad().ReadMatchingFrom(db);

                all.Count().Should().Be(3);

                var w2doodads = await new Doodad { PrimaryWidgetId = w2.In(db) }.ReadMatchingFrom(db);

                (w2doodads.Any(d => d.Name == "B") && w2doodads.Any(d => d.Name == "C") && w2doodads.Count() == 2)
                    .Should().BeTrue();

                await new Doodad { Name = "B" }.DeleteMatchingFrom(db);

                w2doodads = await new Doodad { PrimaryWidgetId = w2.In(db) }.ReadMatchingFrom(db);

                (w2doodads.Any(d => d.Name == "C") && w2doodads.Count() == 1)
                    .Should().BeTrue();
            });
        }
    }
}

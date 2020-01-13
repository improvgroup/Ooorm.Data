using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System;

namespace Ooorm.Data.SqlServer.Tests
{    
    public class IntegrationTest
    {
        public class Doodad : DbItem<Doodad, int>
        {
            public string Name { get; set; }
            public DbRef<Widget, int> PrimaryWidgetId { get; set; }
        }

        public class Widget : DbItem<Widget, int>
        {
            public int Value { get; set; }
        }

        public class WidgetDoodad : DbItem<WidgetDoodad, long>
        {
            public DbVal<Widget, int> WidgetId { get; set; }
            public DbVal<Doodad, int> DoodadId { get; set; }
            public bool Active { get; set; }
        }

        [Fact]
        public Task CreateSimpleTable() => TestFixture.WithTempDb(db => db.CreateTable<Widget, int>());


        [Fact]
        public Task CrudTest() => TestFixture.WithTempDb(async db =>
        {
            await Widget.CreateTable(db);
            await Doodad.CreateTable(db);
            await WidgetDoodad.CreateTable(db);

            var widget1 = new Widget { Value = 1 };
            var widget2 = new Widget { Value = 2 };
            var widget3 = new Widget { Value = 4 };

            var results = await db.Write<Widget, int>(widget1, widget2, widget3);

            results.Count.Should().Be(3);
            results.Keys.Should().Contain(new int[] { widget1.ID, widget2.ID, widget3.ID });
            widget1.ID.Should().BeGreaterThan(0);
            widget2.ID.Should().BeGreaterThan(0);
            widget3.ID.Should().BeGreaterThan(0);
            widget1.ID.Should().NotBe(widget2.ID);
            widget2.ID.Should().NotBe(widget3.ID);

            var widgets = await db.Read<Widget, int>();
            foreach (var ab in widgets.Zip(new Widget[] { widget1, widget2, widget3 }, (a, b) => (a, b)))
                ab.a.Should().BeEquivalentTo(ab.b);

            var widget2_read = await db.Read<Widget, int>(widget2.ID);
            widget2_read.Should().BeEquivalentTo(widget2);

            var doodad1 =
                await new Doodad
                {
                    Name = "First",
                    PrimaryWidgetId = widget1.In(db)
                }.WriteTo(db);

            var doodad2 =
                await new Doodad
                {
                    Name = "Second",
                    PrimaryWidgetId = widget3.In(db)
                }.WriteTo(db);

            await new WidgetDoodad
            {
                DoodadId = doodad1.In(db),
                WidgetId = widget1.In(db),
                Active = true,
            }.WriteTo(db);

            await new WidgetDoodad
            {
                DoodadId = doodad2.In(db),
                WidgetId = widget2.In(db),
                Active = true,
            }.WriteTo(db);

            await new WidgetDoodad
            {
                DoodadId = doodad1.In(db),
                WidgetId = widget2.In(db),
                Active = true,
            }.WriteTo(db);

            var linksToWidget2 = await WidgetDoodad.Read<long?>((r, p) => r.WidgetId == p).With(widget2.ID).From(db);
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

        [Fact]
        public Task ItemExtensionsTest() => TestFixture.WithTempDb(async db =>
        {
            await Widget.CreateTable(db);
            await Doodad.CreateTable(db);
            await WidgetDoodad.CreateTable(db);

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

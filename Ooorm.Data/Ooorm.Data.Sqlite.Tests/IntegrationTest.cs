using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using Ooorm.Data.Matching;
using Ooorm.Data.TestUtils;

namespace Ooorm.Data.Sqlite.Tests
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
        public async Task CreateSimpleTable()
        {
            using var temp = new TestFixture.TempSqliteDb();
            await temp.db.CreateTable<Widget, int>();
        }


        [Fact]
        public async Task CrudTest()
        {
            using var temp = new TestFixture.TempSqliteDb();

            await Widget.CreateTable(temp.db);
            await Doodad.CreateTable(temp.db);
            await WidgetDoodad.CreateTable(temp.db);

            var widget1 = new Widget { Value = 1 };
            var widget2 = new Widget { Value = 2 };
            var widget3 = new Widget { Value = 4 };

            var results = await temp.db.Write<Widget, int>(widget1, widget2, widget3);

            results.Count.Should().Be(3);
            results.Keys.Should().Contain(new int[] { widget1.ID, widget2.ID, widget3.ID });
            widget1.ID.Should().BeGreaterThan(0);
            widget2.ID.Should().BeGreaterThan(0);
            widget3.ID.Should().BeGreaterThan(0);
            widget1.ID.Should().NotBe(widget2.ID);
            widget2.ID.Should().NotBe(widget3.ID);

            var widgets = await temp.db.Read<Widget, int>();
            foreach (var ab in widgets.Zip(new Widget[] { widget1, widget2, widget3 }, (a, b) => (a, b)))
                ab.a.Should().BeEquivalentTo(ab.b, PublicOnly.Rule<Widget>());

            var widget2_read = await temp.db.Read<Widget, int>(widget2.ID);
            widget2_read.Should().BeEquivalentTo(widget2, PublicOnly.Rule<Widget>());

            var doodad1 =
                await new Doodad
                {
                    Name = "First",
                    PrimaryWidgetId = widget1.In(temp.db)
                }.WriteTo(temp.db);

            var doodad2 =
                await new Doodad
                {
                    Name = "Second",
                    PrimaryWidgetId = widget3.In(temp.db)
                }.WriteTo(temp.db);

            await new WidgetDoodad
            {
                DoodadId = doodad1.In(temp.db),
                WidgetId = widget1.In(temp.db),
                Active = true,
            }.WriteTo(temp.db);

            await new WidgetDoodad
            {
                DoodadId = doodad2.In(temp.db),
                WidgetId = widget2.In(temp.db),
                Active = true,
            }.WriteTo(temp.db);

            await new WidgetDoodad
            {
                DoodadId = doodad1.In(temp.db),
                WidgetId = widget2.In(temp.db),
                Active = true,
            }.WriteTo(temp.db);

            var linksToWidget2 = await WidgetDoodad.Read<long?>((r, p) => r.WidgetId == p).With(widget2.ID).From(temp.db);
            linksToWidget2.Count.Should().Be(2);

            var first = await temp.db.Dereference(linksToWidget2.First().DoodadId);
            var last = await temp.db.Dereference(linksToWidget2.Last().DoodadId);

            if (first.ID == doodad1.ID)
                last.ID.Should().Be(doodad2.ID);
            else if (first.ID == doodad2.ID)
                last.ID.Should().Be(doodad1.ID);
            else
                false.Should().BeTrue();            
        }

        [Fact]
        public async Task TestReadMatchingWithBasicEquality()
        {
            using var temp = new TestFixture.TempSqliteDb();

            // --- assemble ---
            await Widget.CreateTable(temp.db);
            await Doodad.CreateTable(temp.db);
            await WidgetDoodad.CreateTable(temp.db);

            // Create Widgets and Doodads
            var w1 = await new Widget { Value = 100 }.WriteTo(temp.db);
            var w2 = await new Widget { Value = 200 }.WriteTo(temp.db);
            await new Doodad { Name = "A", PrimaryWidgetId = w1.In(temp.db) }.WriteTo(temp.db);
            await new Doodad { Name = "B", PrimaryWidgetId = w2.In(temp.db) }.WriteTo(temp.db);
            await new Doodad { Name = "C", PrimaryWidgetId = w2.In(temp.db) }.WriteTo(temp.db);

            // Read all Doodads with Name "A"
            var matchingA = await Doodad.ReadMatching(() => new Doodad { Name = "A" }).From(temp.db);
            var a = matchingA.Single();

            a.Name.Should().Be("A");
            var w1FromA = await a.PrimaryWidgetId.Get();
            w1FromA.Value.Should().Be(100);

            // Read all Doodads matching "no constraints"
            var all = await Doodad.ReadMatching(() => new Doodad { }).From(temp.db);
            all.Should().HaveCount(3);

            // Read all Doodads with PrimaryWidget w2
            var doodadsWithWidget2 = Doodad.ReadMatching(() => new Doodad { PrimaryWidgetId = w2.In(temp.db) });
            var w2Doodads = await doodadsWithWidget2.From(temp.db);

            w2Doodads.Should().Contain(d => d.Name == "B");
            w2Doodads.Should().Contain(d => d.Name == "C");
            w2Doodads.Should().HaveCount(2);

            // Delete all Doodads with Name "B"
            await Doodad.DeleteMatching(() => new Doodad { Name = "B" }).From(temp.db);

            // Refresh doodads 2 list after change
            w2Doodads = await doodadsWithWidget2.From(temp.db);

            // After delete only doodad "C" has PrimaryWidget w2
            w2Doodads.Should().ContainSingle(d => d.Name == "C");
            w2Doodads.Should().HaveCount(1);
        }

        [Fact]
        public async Task TestReadMatchingWithComparisons()
        {
            using var temp = new TestFixture.TempSqliteDb();

            // --- assemble ---
            await Widget.CreateTable(temp.db);
            var w1 = await new Widget { Value = 100 }.WriteTo(temp.db);
            var w2 = await new Widget { Value = 200 }.WriteTo(temp.db);
            var w3 = await new Widget { Value = 300 }.WriteTo(temp.db);

            // --- act ---
            var readLessThan200 = Widget.ReadMatching(() => new Widget { Value = (LessThan<int>) 200 });
            var lt200 = await readLessThan200.From(temp.db);
            // --- assert ---
            lt200.Should().ContainSingle(w => w.Value == 100);
            lt200.Should().HaveCount(1);

            // --- act ---
            var readLessThanEqualTo200 = Widget.ReadMatching(() => new Widget { Value = (NotGreaterThan<int>) 200 });
            var lte200 = await readLessThanEqualTo200.From(temp.db);
            // --- assert ---
            lte200.Should().Contain(w => w.Value == 100);
            lte200.Should().Contain(w => w.Value == 200);
            lte200.Should().HaveCount(2);

            // --- act ---
            var readGreaterThan200 = Widget.ReadMatching(() => new Widget { Value = (GreaterThan<int>) 200 });
            var gt200 = await readGreaterThan200.From(temp.db);
            // --- assert ---
            gt200.Should().ContainSingle(w => w.Value == 300);
            gt200.Should().HaveCount(1);

            // --- act ---
            var readGreaterThanEqualTo200 = Widget.ReadMatching(() => new Widget { Value = (NotLessThan<int>) 200 });
            var gte200 = await readGreaterThanEqualTo200.From(temp.db);
            // --- assert ---
            gte200.Should().Contain(w => w.Value == 200);
            gte200.Should().Contain(w => w.Value == 300);
            gte200.Should().HaveCount(2);
        }
    }
}

using FluentAssertions;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

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
            public int Values { get; set; }
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
            await using var temp = await TestFixture.TempDb();
            await temp.db.CreateTable<Widget, int>();
        }


        [Fact]
        public async Task CrudTest()
        {
            await using var temp = await TestFixture.TempDb();

            // Create tables in temp database 'db'
            await Widget.CreateTable(temp.db);
            await Doodad.CreateTable(temp.db);
            await WidgetDoodad.CreateTable(temp.db);

            // Instantiate widget entities
            var widget1 = new Widget { Values = 1 };
            var widget2 = new Widget { Values = 2 };
            var widget3 = new Widget { Values = 4 };

            // Write widget entities to database
            var results = await temp.db.Write<Widget, int>(widget1, widget2, widget3);

            // Assert that Widget IDs are set to what the database assigned
            // and that the write method returns the correct items
            results.Count.Should().Be(3);
            results.Keys.Should().Contain(new int[] { widget1.ID, widget2.ID, widget3.ID });
            widget1.ID.Should().BeGreaterThan(0);
            widget2.ID.Should().BeGreaterThan(0);
            widget3.ID.Should().BeGreaterThan(0);
            widget1.ID.Should().NotBe(widget2.ID);
            widget2.ID.Should().NotBe(widget3.ID);

            // Read all widgets from db
            var widgets = await temp.db.Read<Widget, int>();

            // Assert that exactly the right widgets were returned, in the order they were written
            foreach (var ab in widgets.Zip(new Widget[] { widget1, widget2, widget3 }, (a, b) => (a, b)))
                ab.a.Should().BeEquivalentTo(ab.b);

            // Reading by ID should return the correct item
            var widget2_read = await temp.db.Read<Widget, int>(widget2.ID);
            widget2_read.Should().BeEquivalentTo(widget2);

            // Create 'Doodads' and 'WidgetDoodads'
            var doodad1 = await new Doodad
            {
                Name = "First",
                PrimaryWidgetId = widget1.In(temp.db) // '.In' method returns a DbRef to an entity
            }.WriteTo(temp.db);

            var doodad2 = await new Doodad
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

            // Read WidgetDoodads where "row WidgetId = parameter" with parameter of widget2.ID from the database 'db'
            var linksToWidget2 = await WidgetDoodad.Read<long?>((row, param) => row.WidgetId == param).With(widget2.ID).From(temp.db);
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
    }
}

using FluentAssertions;
using Ooorm.Data.Attributes;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace Ooorm.Data.SqlServer.Tests
{
    public class LocalHostTests
    {
        private readonly SqlServerConnectionSource connection;
        private static readonly SqlServerConnectionSource localmaster = SqlServerConnectionSource.CreateSharedSource(TestFixture.LocalMasterConnectionString);
        private readonly IDatabase database;
        private static readonly IDatabase master = new SqlDatabase(localmaster);

        public class Doodad : IDbItem
        {
            [Id]
            [Column(nameof(ID))]
            public int ID { get; set; }
            [Column(nameof(Name))]
            public string Name { get; set; }
            [Column(nameof(PrimaryWidgetId))]
            public DbRef<Widget> PrimaryWidgetId { get; set; }
        }

        public class Widget : IDbItem
        {
            [Id]
            [Column(nameof(ID))]
            public int ID { get; set; }
            [Column(nameof(Value))]
            public int Value { get; set; }
        }

        public class WidgetDoodad : IDbItem
        {
            [Id]
            [Column(nameof(ID))]
            public int ID { get; set; }
            [Column(nameof(WidgetId))]
            public DbVal<Widget> WidgetId { get; set; }
            [Column(nameof(DoodadId))]
            public DbVal<Doodad> DoodadId { get; set; }
        }

        public LocalHostTests()
        {
            Task.Run(async () =>
            {
                await master.DropDatabase(TestFixture.TestDbName);
                await master.CreateDatabase(TestFixture.TestDbName);
                return 1;
            }).Result.Should().Be(1);
            connection = SqlServerConnectionSource.CreateSharedSource(TestFixture.ConnectionString);
            database = new SqlDatabase(connection);
            Task.Run(async () =>
            {
                await database.CreateTable<Doodad>();
                await database.CreateTable<Widget>();
                await database.CreateTable<WidgetDoodad>();
                 return 1;
            }).Result.Should().Be(1);
        }

        [Fact]
        public async Task CrudTest()
        {
            var widget1 = new Widget { Value = 1 };
            var widget2 = new Widget { Value = 2 };
            var widget3 = new Widget { Value = 4 };

            var count = await database.Write(widget1, widget2, widget3);

            widget1.ID.Should().BeGreaterThan(0);
            widget2.ID.Should().BeGreaterThan(0);
            widget3.ID.Should().BeGreaterThan(0);
            widget1.ID.Should().NotBe(widget2.ID);
            widget2.ID.Should().NotBe(widget3.ID);
            count.Should().Be(3);

            var widgets = await database.Read<Widget>();
            foreach (var ab in widgets.Zip(new Widget[] { widget1, widget2, widget3 }, (a, b) => (a, b)))
                ab.a.Should().BeEquivalentTo(ab.b);

            (await database.Read<Widget>(widget2.ID)).Should().BeEquivalentTo(widget2);

            var doodad1 = new Doodad
            {
                Name = "First",
                PrimaryWidgetId = widget1
            };

            var doodad2 = new Doodad
            {
                Name = "Second",
                PrimaryWidgetId = widget3
            };

            await database.Write(doodad1, doodad2);

            await database.Write(
                new WidgetDoodad
                {
                    DoodadId = doodad1,
                    WidgetId = widget1,
                },
                new WidgetDoodad
                {
                    DoodadId = doodad2,
                    WidgetId = widget2,
                },
                new WidgetDoodad
                {
                    DoodadId = doodad1,
                    WidgetId = widget2,
                });

            var linksToWidget2 = (await database.Read<WidgetDoodad, int?>((r, p) => r.WidgetId == p, widget2.ID)).ToList();
            linksToWidget2.Count.Should().Be(2);

            var first = await database.Dereference(linksToWidget2.First().DoodadId);
            var last = await database.Dereference(linksToWidget2.Last().DoodadId);

            if (first.ID == doodad1.ID)
                last.ID.Should().Be(doodad2.ID);
            else if (first.ID == doodad2.ID)
                last.ID.Should().Be(doodad1.ID);
            else
                false.Should().BeTrue();
        }

        ~LocalHostTests()
        {
            connection.Dispose();
        }
    }
}

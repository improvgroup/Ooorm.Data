using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Ooorm.Data.SqlServer.Tests
{
    public class LocalHostTests
    {
        private static readonly SqlServerConnectionSource connection = SqlServerConnectionSource.CreateSharedSource(TestFixture.ConnectionString);
        private static readonly IDatabase database = new SqlDatabase(connection);

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
        }

        public LocalHostTests()
        {

        }

        public void CreateRecords()
        {
            var widget1 = new Widget { Value = 1 };
            var widget2 = new Widget { Value = 2 };
            var widget3 = new Widget { Value = 4 };

            database.Create(widget1, widget2, widget3);

            widget1.ID.Should().BeGreaterThan(0);
            widget2.ID.Should().BeGreaterThan(0);
            widget3.ID.Should().BeGreaterThan(0);
            widget1.ID.Should().NotBe(widget2.ID);
            widget2.ID.Should().NotBe(widget3.ID);

            var doodad1 = new Doodad
            {
                Name = "First",
                PrimaryWidgetId = widget1.ID
            };

            var doodad2 = new Doodad
            {
                Name = "Second",
                PrimaryWidgetId = widget3.ID
            };

            database.Create(doodad1, doodad2);

            var item1 = new WidgetDoodad
            {
                DoodadId = doodad1,
                WidgetId = widget1,
            }
        }
    }
}

using FluentAssertions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ooorm.Data.SignalrClient;
using Ooorm.Data.SignalrHub;
using Ooorm.Data.Volatile;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ooorm.Data.Signalr.Tests
{
    public class HubClientTests
    {
        const string url = "https://localhost:50002";

        static HubClientTests()
        {
            Task.Run(() =>
                WebHost
                    .CreateDefaultBuilder()
                    .UseUrls(url)
                    .UseStartup<Startup>()
                    .Build()
                    .Run());
            Thread.Sleep(3000);
        }

        //[Fact]
        public async Task CanAddAndReadItems()
        {
            var db = new SignalrDatabase(url);
            await db.LoadTable<DatabaseItem>();
            await db.LoadTable<ItemMetadata>();

            await db.Write(new DatabaseItem { Message = nameof(CanAddAndReadItems), });

            (await db.Read<DatabaseItem>())
                .Any(i => i.Message == nameof(CanAddAndReadItems))
                .Should()
                .BeTrue();
        }
    }

    public class DatabaseItem : IDbItem
    {
        public int ID { get; set; }
        public string Message { get; set; }
    }

    public class ItemMetadata : IDbItem
    {
        public int ID { get; set; }
        public string Content { get; set; }
        public DbVal<DatabaseItem> Item { get; set; }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var db = new VolatileDatabase();
            Task.Run(async () =>
            {
                await db.CreateTable<DatabaseItem>();
                await db.CreateTable<ItemMetadata>();

                var (first, second, third, fourth) = (
                    new DatabaseItem { Message = "First" },
                    new DatabaseItem { Message = "Second" },
                    new DatabaseItem { Message = "Third" },
                    new DatabaseItem { Message = "Fourth" });

                await db.Write(first, second, third, fourth);

                await db.Write(
                    new ItemMetadata { Content = "A", Item = first.In(db) },
                    new ItemMetadata { Content = "B", Item = first.In(db) },
                    new ItemMetadata { Content = "C", Item = second.In(db) });


            }).Wait();

            services.AddTransient<IDatabase>((s) => db);
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) =>
            app.UseSignalR(routes => routes.MapHub<OoormHub<DatabaseItem>>($"/{nameof(DatabaseItem)}"));
    }
}

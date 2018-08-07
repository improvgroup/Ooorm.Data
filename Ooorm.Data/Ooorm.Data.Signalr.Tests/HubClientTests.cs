using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Ooorm.Data.SignalrClient;
using Ooorm.Data.SignalrHub;
using Ooorm.Data.Volatile;
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

        }

        [Fact]
        public async Task CanAddAndReadItems()
        {
            var db = new SignalrDatabase(url);

            await db.Read<DatabaseItem>();
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
            }).Wait();

            services.AddTransient<IDatabase>((s) => db);
            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) =>
            app.UseSignalR(routes => routes.MapHub<OoormHub<DatabaseItem>>($"/{nameof(DatabaseItem)}"));
    }
}

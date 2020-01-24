using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer.Tests
{
    public class TestFixture
    {
        public static readonly string TestDbName = "OoormDataTestDb";

        public static readonly string Server = "localhost";

        // Recommended method of setting up a test instance;
        // https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017
        public static string ConnectionString(string db) => $"Server=localhost;Database={db};User Id=SA;Password=TransientTestDb!;";

        public static async Task<TempMssqlDb> TempDb([CallerMemberName] string name = null)
        {
            string dbName = "OoormSqlTests" + name + Guid.NewGuid().ToString().Substring(0, 8);
            var master = new SqlDatabase(SqlConnection.CreateShared(ConnectionString("master")));
            await master.DropDatabase(dbName);
            await master.CreateDatabase(dbName);

            return new TempMssqlDb(new SqlDatabase(SqlConnection.CreateShared(ConnectionString(dbName))), async () => await master.DropDatabase(dbName));            
        }

        public class TempMssqlDb : IAsyncDisposable
        {
            private readonly Func<Task> drop;
            public readonly IDatabase db;

            public TempMssqlDb(IDatabase db, Func<Task> drop) => (this.db, this.drop) = (db, drop);
            
            public async ValueTask DisposeAsync() => await drop();
        }
    }
}

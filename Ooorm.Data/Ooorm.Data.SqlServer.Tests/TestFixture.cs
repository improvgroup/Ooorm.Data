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

        public static async Task WithTempDb(Func<IDatabaseManagementSystem, Task> action, [CallerMemberName] string name = null)
        {
            string dbName = "OoormSqlTests" + name + Guid.NewGuid().ToString().Substring(0, 8);
            var master = new SqlDatabase(SqlConnection.CreateShared(ConnectionString("master")));
            await master.DropDatabase(dbName);
            await master.CreateDatabase(dbName);
            try
            {
                var db = new SqlDatabase(SqlConnection.CreateShared(ConnectionString(dbName)));
                await action(db);
            }
            catch (Exception) { throw; }
            finally
            {
                await master.DropDatabase(dbName);
            }
        }
    }
}

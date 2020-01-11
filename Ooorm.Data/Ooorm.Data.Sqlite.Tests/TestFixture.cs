using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite.Tests
{
    public class TestFixture
    {
        public static readonly string TestDbName = "OoormDataTestDb";

        public static readonly string Server = "localhost";

        // Recommended method of settign up a test instance;
        // https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017
        const string ConnectionString = "Data Source=:memory:;Version=3;New=True;";

        public static async Task WithTempDb(Func<IDatabase, Task> action, [CallerMemberName] string name = null)
        {                        
            var db = new SqliteDatabase(SqliteConnection.CreateShared(ConnectionString));
            await action(db);        
        }
    }
}

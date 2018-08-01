namespace Ooorm.Data.SqlServer.Tests
{
    public class TestFixture
    {
        public static readonly string TestDbName = "OoormDataTestDb";

        public static readonly string Server = "localhost";

        // Recommended method of settign up a test instance;
        // https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017
        public static string ConnectionString(string db) => $"Server=localhost;Database={db};Integrated Security=True;";
    }
}

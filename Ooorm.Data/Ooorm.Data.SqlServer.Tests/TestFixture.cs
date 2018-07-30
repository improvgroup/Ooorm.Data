namespace Ooorm.Data.SqlServer.Tests
{
    public class TestFixture
    {
        public static readonly string TestDbName = "OoormDataTestDb";

        public static readonly string Server = "localhost";

        // Recommended method of settign up a test instance;
        // https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017
        public static readonly string ConnectionString = "Server=localhost;Database=OoormDataTestDb;Integrated Security=True;";

        public static readonly string LocalMasterConnectionString = "Server=localhost;Database=master;Integrated Security=True;";
    }
}

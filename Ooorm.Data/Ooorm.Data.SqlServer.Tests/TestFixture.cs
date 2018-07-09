namespace Ooorm.Data.SqlServer.Tests
{
    public class TestFixture
    {
        // Recommended method of settign up a test instance; 
        // https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-linux-2017
        public static string ConnectionString = "Server=localhost;Database=master;User Id=SA;Password=PublicPassword;";
    }
}

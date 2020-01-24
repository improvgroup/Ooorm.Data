using System;

namespace Ooorm.Data.Sqlite.Tests
{
    public class TestFixture
    {
        public static readonly string TestDbName = "OoormDataTestDb";

        public static readonly string ConnectionString = $"Data Source={TestDbName};Mode=Memory;Cache=Private";

        public class TempSqliteDb : IDisposable
        {
            public readonly IDatabase db;

            public TempSqliteDb() => db = new SqliteDatabase(SqliteConnection.CreateShared(ConnectionString));

            public void Dispose() { }
        }

    }
}

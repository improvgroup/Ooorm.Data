using System;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public class SharedConnectionSource : SqliteConnection
    {
        private readonly Microsoft.Data.Sqlite.SqliteConnection Connection;

        public SharedConnectionSource(string connectionString) : base(connectionString)
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            connection.Open();
            OpenConnections = 1;
            Connection = connection;
        }

        public override void Dispose()
        {
            Connection.Close();
            OpenConnections = 0;
        }

        public override void WithConnection(Action<Microsoft.Data.Sqlite.SqliteConnection> action) =>
            action(Connection);


        public override Task WithConnectionAsync(Action<Microsoft.Data.Sqlite.SqliteConnection> action)
        {
            action(Connection);
            return Task.CompletedTask;
        }


        public override Task WithConnectionAsync(Func<Microsoft.Data.Sqlite.SqliteConnection, Task> action) =>
            action(Connection);


        public override T FromConnection<T>(Func<Microsoft.Data.Sqlite.SqliteConnection, T> action) =>
            action(Connection);


        public override Task<T> FromConnectionAsync<T>(Func<Microsoft.Data.Sqlite.SqliteConnection, Task<T>> action) =>
            action(Connection);

    }

}

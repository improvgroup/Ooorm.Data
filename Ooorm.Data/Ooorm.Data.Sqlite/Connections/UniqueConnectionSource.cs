using System;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public class UniqueConnectionSource : SqliteConnection
    {
        public UniqueConnectionSource(string connectionString) : base(connectionString) { }

        public override void Dispose() { }

        public override void WithConnection(Action<Microsoft.Data.Sqlite.SqliteConnection> action)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            connection.Open();
            OpenConnections++;
            action(connection);
            connection.Close();
            OpenConnections--;
        }

        public override async Task WithConnectionAsync(Action<Microsoft.Data.Sqlite.SqliteConnection> action)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await connection.OpenAsync();
            OpenConnections++;
            action(connection);
            connection.Close();
            OpenConnections--;
        }

        public override async Task WithConnectionAsync(Func<Microsoft.Data.Sqlite.SqliteConnection, Task> action)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await connection.OpenAsync();
            OpenConnections++;
            await action(connection);
            connection.Close();
            OpenConnections--;
        }

        public override T FromConnection<T>(Func<Microsoft.Data.Sqlite.SqliteConnection, T> action)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            connection.Open();
            OpenConnections++;
            var value = action(connection);
            connection.Close();
            OpenConnections--;
            return value;
        }

        public override async Task<T> FromConnectionAsync<T>(Func<Microsoft.Data.Sqlite.SqliteConnection, Task<T>> action)
        {
            using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
            await connection.OpenAsync();
            OpenConnections++;
            var value = await action(connection);
            connection.Close();
            OpenConnections--;
            return value;
        }
    }

}

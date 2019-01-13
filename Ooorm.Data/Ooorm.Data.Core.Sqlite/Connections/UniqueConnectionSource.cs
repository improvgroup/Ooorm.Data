using System;
using System.Threading.Tasks;

namespace Ooorm.Data.Core.Sqlite
{
    public class UniqueConnectionSource : SqliteConnection
    {
        public UniqueConnectionSource(string connectionString) : base(connectionString) { }

        public override void Dispose() { }

        public override void WithConnection(Action<System.Data.SQLite.SQLiteConnection> action)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                connection.Open();
                OpenConnections++;
                action(connection);
                connection.Close();
                OpenConnections--;
            }
        }

        public override async Task WithConnectionAsync(Action<System.Data.SQLite.SQLiteConnection> action)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                OpenConnections++;
                action(connection);
                connection.Close();
                OpenConnections--;
            }
        }

        public override async Task WithConnectionAsync(Func<System.Data.SQLite.SQLiteConnection, Task> action)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                OpenConnections++;
                await action(connection);
                connection.Close();
                OpenConnections--;
            }
        }

        public override T FromConnection<T>(Func<System.Data.SQLite.SQLiteConnection, T> action)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                connection.Open();
                OpenConnections++;
                var value = action(connection);
                connection.Close();
                OpenConnections--;
                return value;
            }
        }

        public override async Task<T> FromConnectionAsync<T>(Func<System.Data.SQLite.SQLiteConnection, Task<T>> action)
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                OpenConnections++;
                var value = await action(connection);
                connection.Close();
                OpenConnections--;
                return value;
            }
        }
    }

}

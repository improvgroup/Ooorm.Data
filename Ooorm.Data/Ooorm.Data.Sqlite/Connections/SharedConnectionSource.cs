using System;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public class SharedConnectionSource : SqliteConnection
    {
        private readonly Task<System.Data.SQLite.SQLiteConnection> Connection;

        public SharedConnectionSource(string connectionString) : base(connectionString)
        {
            Connection = Task.Run(async () =>
            {
                var connection = new System.Data.SQLite.SQLiteConnection(connectionString);
                await connection.OpenAsync();
                OpenConnections = 1;
                return connection;
            });
        }

        public override void Dispose()
        {
            Connection.Result.Close();
            OpenConnections = 0;
        }

        public override void WithConnection(Action<System.Data.SQLite.SQLiteConnection> action) =>
            action(Connection.Result);


        public override async Task WithConnectionAsync(Action<System.Data.SQLite.SQLiteConnection> action) =>
            action(await Connection);


        public override async Task WithConnectionAsync(Func<System.Data.SQLite.SQLiteConnection, Task> action) =>
            await action(await Connection);


        public override T FromConnection<T>(Func<System.Data.SQLite.SQLiteConnection, T> action) =>
            action(Connection.Result);


        public override async Task<T> FromConnectionAsync<T>(Func<System.Data.SQLite.SQLiteConnection, Task<T>> action) =>
            await action(await Connection);

    }

}

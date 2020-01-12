using Ooorm.Data.ConnectionManagement;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public abstract class SqliteConnection : IConnectionSource<SQLiteConnection>
    {
        protected readonly string connectionString;

        public SqliteConnection(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int OpenConnections { get; protected set; }

        public bool CanConnect
        {
            get
            {
                using var connection = new SQLiteConnection(connectionString);
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SQLiteException)
                {
                    return false;
                }
            }
        }

        public abstract void WithConnection(Action<System.Data.SQLite.SQLiteConnection> action);
        public abstract Task WithConnectionAsync(Action<System.Data.SQLite.SQLiteConnection> action);
        public abstract Task WithConnectionAsync(Func<System.Data.SQLite.SQLiteConnection, Task> action);

        public abstract T FromConnection<T>(Func<System.Data.SQLite.SQLiteConnection, T> action);
        public abstract Task<T> FromConnectionAsync<T>(Func<System.Data.SQLite.SQLiteConnection, Task<T>> action);

        public abstract void Dispose();

        /// <summary>
        /// Returns a connection source that creates a new connection upon every request
        /// </summary>
        public static UniqueConnectionSource CreateTransient(string connectionString) => new UniqueConnectionSource(connectionString);

        /// <summary>
        /// Returns a connection source that keeps 1 connection open and uses it for every request
        /// </summary>
        public static SharedConnectionSource CreateShared(string connectionString) => new SharedConnectionSource(connectionString);
    }

}

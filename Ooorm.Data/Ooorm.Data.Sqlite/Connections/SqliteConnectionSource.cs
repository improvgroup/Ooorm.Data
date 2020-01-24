using Ooorm.Data.ConnectionManagement;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public abstract class SqliteConnection : IConnectionSource<Microsoft.Data.Sqlite.SqliteConnection>
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
                using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);
                try
                {
                    connection.Open();
                    return true;
                }
                catch (SqliteException)
                {
                    return false;
                }
            }
        }

        public abstract void WithConnection(Action<Microsoft.Data.Sqlite.SqliteConnection> action);
        public abstract Task WithConnectionAsync(Action<Microsoft.Data.Sqlite.SqliteConnection> action);
        public abstract Task WithConnectionAsync(Func<Microsoft.Data.Sqlite.SqliteConnection, Task> action);

        public abstract T FromConnection<T>(Func<Microsoft.Data.Sqlite.SqliteConnection, T> action);
        public abstract Task<T> FromConnectionAsync<T>(Func<Microsoft.Data.Sqlite.SqliteConnection, Task<T>> action);

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

using Ooorm.Data.ConnectionManagement;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    public abstract class SqlConnection : IConnectionSource<System.Data.SqlClient.SqlConnection>
    {
        protected readonly string connectionString;

        public SqlConnection(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int OpenConnections { get; protected set; }

        public bool CanConnect
        {
            get
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        return true;
                    }
                    catch (SqlException)
                    {
                        return false;
                    }
                }
            }
        }

        public abstract void WithConnection(Action<System.Data.SqlClient.SqlConnection> action);
        public abstract Task WithConnectionAsync(Action<System.Data.SqlClient.SqlConnection> action);
        public abstract Task WithConnectionAsync(Func<System.Data.SqlClient.SqlConnection, Task> action);

        public abstract T FromConnection<T>(Func<System.Data.SqlClient.SqlConnection, T> action);
        public abstract Task<T> FromConnectionAsync<T>(Func<System.Data.SqlClient.SqlConnection, Task<T>> action);

        internal Task<Task<IEnumerable<IDbItem>>> FromConnectionAsync(Func<System.Data.SqlClient.SqlConnection, Task<object>> p)
        {
            throw new NotImplementedException();
        }

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

using Ooorm.Data.ConnectionManagement;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    public abstract class SqlServerConnectionSource : IConnectionSource<SqlConnection>
    {
        protected readonly string connectionString;

        public SqlServerConnectionSource(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public int OpenConnections { get; protected set; }

        public bool CanConnect
        {
            get
            {
                using (var connection = new SqlConnection(connectionString))
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
            
        public abstract void WithConnection(Action<SqlConnection> action);
        public abstract Task WithConnectionAsync(Action<SqlConnection> action);
        public abstract T FromConnection<T>(Func<SqlConnection, T> action);
        public abstract Task<T> FromConnectionAsync<T>(Func<SqlConnection, T> action);

        internal Task<Task<IEnumerable<IDbItem>>> FromConnectionAsync(Func<SqlConnection, Task<object>> p)
        {
            throw new NotImplementedException();
        }

        public abstract void Dispose();

        /// <summary>
        /// Returns a connection source that creates a new connection upon every request
        /// </summary>        
        public static UniqueConnectionSource CreateUniqueSource(string connectionString) => new UniqueConnectionSource(connectionString);

        /// <summary>
        /// Returns a connection source that keeps 1 connection open and uses it for every request
        /// </summary>        
        public static SharedConnectionSource CreateSharedSource(string connectionString) => new SharedConnectionSource(connectionString);
    }

}

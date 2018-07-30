using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    public class UniqueConnectionSource : SqlServerConnectionSource
    {
        public UniqueConnectionSource(string connectionString) : base(connectionString) { }

        public override void Dispose() { }

        public override void WithConnection(Action<SqlConnection> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                OpenConnections++;
                action(connection);
                connection.Close();
                OpenConnections--;
            }
        }

        public override async Task WithConnectionAsync(Action<SqlConnection> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                OpenConnections++;
                action(connection);
                connection.Close();
                OpenConnections--;
            }
        }

        public override async Task WithConnectionAsync(Func<SqlConnection, Task> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                OpenConnections++;
                await action(connection);
                connection.Close();
                OpenConnections--;
            }
        }

        public override T FromConnection<T>(Func<SqlConnection, T> action)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                OpenConnections++;
                var value = action(connection);
                connection.Close();
                OpenConnections--;
                return value;
            }
        }

        public override async Task<T> FromConnectionAsync<T>(Func<SqlConnection, Task<T>> action)
        {
            using (var connection = new SqlConnection(connectionString))
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

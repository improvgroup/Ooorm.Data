using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    public class SharedConnectionSource : SqlConnection
    {
        private readonly Task<System.Data.SqlClient.SqlConnection> Connection;

        public SharedConnectionSource(string connectionString) : base(connectionString)
        {
            Connection = Task.Run(async () =>
            {
                var connection = new System.Data.SqlClient.SqlConnection(connectionString);
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

        public override void WithConnection(Action<System.Data.SqlClient.SqlConnection> action)
        {
            action(Connection.Result);
        }

        public override async Task WithConnectionAsync(Action<System.Data.SqlClient.SqlConnection> action)
        {
            action(await Connection);
        }

        public override async Task WithConnectionAsync(Func<System.Data.SqlClient.SqlConnection, Task> action)
        {
            await action(await Connection);
        }

        public override T FromConnection<T>(Func<System.Data.SqlClient.SqlConnection, T> action)
        {
            return action(Connection.Result);
        }

        public override async Task<T> FromConnectionAsync<T>(Func<System.Data.SqlClient.SqlConnection, Task<T>> action)
        {
            return await action(await Connection);
        }
    }

}

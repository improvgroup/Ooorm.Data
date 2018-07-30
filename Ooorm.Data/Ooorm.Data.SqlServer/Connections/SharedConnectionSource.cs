﻿using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    public class SharedConnectionSource : SqlServerConnectionSource
    {
        private readonly Task<SqlConnection> Connection;

        public SharedConnectionSource(string connectionString) : base(connectionString)
        {
            Connection = Task.Run(async () =>
            {
                var connection = new SqlConnection(connectionString);
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

        public override void WithConnection(Action<SqlConnection> action)
        {
            action(Connection.Result);
        }

        public override async Task WithConnectionAsync(Action<SqlConnection> action)
        {
            action(await Connection);
        }

        public override async Task WithConnectionAsync(Func<SqlConnection, Task> action)
        {
            await action(await Connection);
        }

        public override T FromConnection<T>(Func<SqlConnection, T> action)
        {
            return action(Connection.Result);
        }

        public override async Task<T> FromConnectionAsync<T>(Func<SqlConnection, Task<T>> action)
        {
            return await action(await Connection);
        }
    }

}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    /// <summary>
    /// Data Access Object for Sql Server connections
    /// </summary>
    internal class SqlDao : BaseDao<System.Data.SqlClient.SqlConnection, SqlCommand, SqlDataReader>
    {
        public SqlDao(Func<IDatabase> db) : base(new SqlDataConsumer(), new DefaultTypeProvider(db), db) { }

        public override void AddKeyValuePair(SqlCommand command, string key, object value)
        {
            var paramValue = value;
            if (value is IdConvertable<int> valId)
                paramValue = valId.ToId();
            else if (value is IdConvertable<int?> refId)
                paramValue = refId.ToId();
            command.Parameters.AddWithValue(key, value);
        }

        public override SqlCommand GetCommand(string sql, System.Data.SqlClient.SqlConnection connection) =>
            new SqlCommand(sql, connection) { CommandType = CommandType.Text };

        public async Task<int> ExecuteAsync(System.Data.SqlClient.SqlConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                command.CommandText = sql;
                AddParameters(command, sql, parameter);
                var result = await command.ExecuteNonQueryAsync();
                return result;
            }
        }

        public async Task<int> ExecuteScalarAsync(System.Data.SqlClient.SqlConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                command.CommandText = sql;
                AddParameters(command, sql, parameter);
                return (int)(await command.ExecuteScalarAsync());
            }
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(System.Data.SqlClient.SqlConnection connection, string sql, object parameter) where T : IDbItem
        {
            using (var command = GetCommand(sql, connection))
            {
                CheckColumnCache<T>();
                AddParameters(command, sql, parameter);
                return await ExecuteReaderAsync<T>(command);
            }
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(System.Data.SqlClient.SqlConnection connection, string sql, (string name, object value) parameter) where T : IDbItem
        {
            using (var command = GetCommand(sql, connection))
            {
                CheckColumnCache<T>();
                AddParameters(command, sql, parameter);
                return await ExecuteReaderAsync<T>(command);
            }
        }

        protected async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SqlCommand command) where T : IDbItem =>
            await Task.Run(() => ExecuteReader<T>(command));

        protected override IEnumerable<T> ExecuteReader<T>(SqlCommand command)
        {
            using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                return ParseReader<T>(reader);
        }
    }
}

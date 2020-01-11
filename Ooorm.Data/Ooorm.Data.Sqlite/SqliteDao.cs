using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{

    /// <summary>
    /// Data Access Object for Sql Server connections
    /// </summary>
    internal class SqliteDao : BaseDao<SQLiteConnection, SQLiteCommand, SQLiteDataReader>
    {
        public SqliteDao(Func<IDatabase> db) : base(new SqliteDataConsumer(), new SqliteTypeProvider(db), db) { }

        public override void AddKeyValuePair(SQLiteCommand command, string key, object value)
        {
            var paramValue = value;
            if (value is IdConvertable<int> valId)
                paramValue = valId.ToId();
            else if (value is IdConvertable<int?> refId)
                paramValue = refId.ToId();
            command.Parameters.AddWithValue(key, value);
        }

        public override SQLiteCommand GetCommand(string sql, SQLiteConnection connection) =>
            new SQLiteCommand(sql, connection) { CommandType = CommandType.Text };

        public async Task<int> ExecuteAsync(System.Data.SQLite.SQLiteConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                command.CommandText = sql;
                Console.WriteLine();
                Console.WriteLine("Query:");
                Console.WriteLine(sql);
                Console.WriteLine("Parameter:");
                Console.WriteLine(JsonConvert.SerializeObject(parameter));
                Console.WriteLine();
                AddParameters(command, sql, parameter);
                var result = await command.ExecuteNonQueryAsync();
                return result;
            }
        }

        public async Task<long> ExecuteScalarAsync(System.Data.SQLite.SQLiteConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                command.CommandText = sql;
                AddParameters(command, sql, parameter);
                return (long)(await command.ExecuteScalarAsync());
            }
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(System.Data.SQLite.SQLiteConnection connection, string sql, object parameter) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
        {
            using (var command = GetCommand(sql, connection))
            {
                CheckColumnCache<T>();
                AddParameters(command, sql, parameter);
                return await ExecuteReaderAsync<T>(command);
            }
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(System.Data.SQLite.SQLiteConnection connection, string sql, (string name, object value) parameter) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
        {
            using (var command = GetCommand(sql, connection))
            {
                CheckColumnCache<T>();
                AddParameters(command, sql, parameter);
                return await ExecuteReaderAsync<T>(command);
            }
        }

        protected async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SQLiteCommand command) where T : IDbItem<TId> where TId : struct, IEquatable<TId> =>
            await Task.Run(() => ExecuteReader<T>(command));

        protected override IEnumerable<T> ExecuteReader<T>(SQLiteCommand command)
        {
            using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                return ParseReader<T>(reader);
        }
    }
}

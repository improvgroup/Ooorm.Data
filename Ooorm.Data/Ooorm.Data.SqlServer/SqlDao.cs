using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    /// <summary>
    /// Data Access Object for Sql Server connections
    /// </summary>
    public class SqlDao : BaseDao<SqlConnection, SqlCommand, SqlDataReader>
    {
        public SqlDao() : base(new SqlDataConsumer(), new DefaultTypeProvider()) { }

        public override void AddKeyValuePair(SqlCommand command, string key, object value)
            => command.Parameters.AddWithValue(key, value);
        
        public override SqlCommand GetCommand(string sql, SqlConnection connection)
        {
            return new SqlCommand(sql, connection)
            {
                CommandType = CommandType.Text
            };            
        }        

        public async Task<int> ExecuteAsync(SqlConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                command.CommandText = sql;
                AddParameters(command, sql, parameter);
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> ReadAsync<T>(SqlConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                CheckColumnCache<T>();
                AddParameters(command, sql, parameter);
                return await ExecuteReaderAsync<T>(command);
            }
        }

        protected async Task<IEnumerable<T>> ExecuteReaderAsync<T>(SqlCommand command)
        {
            using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                return await Task.Run(() => ExecuteReader<T>(command).ToList());
        }

        protected override IEnumerable<T> ExecuteReader<T>(SqlCommand command)
        {
            using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                return ParseReader<T>(reader);
        }        
    }    
}

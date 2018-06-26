using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    internal static class DataAccessObject
    {
        internal static readonly Dictionary<Type, List<Property>> propertyCache = new Dictionary<Type, List<Property>>();

        internal static void AddParameters(SqlCommand command, string sql, object parameter)
        {
            if (parameter == null)
                return;
            var paramType = parameter.GetType();
            if (!propertyCache.ContainsKey(paramType))
                propertyCache[paramType] = paramType.GetDataProperties().ToList();
                            
            foreach (var value in propertyCache[paramType].Where(p => sql.Contains($"@{p.PropertyName}")))
                command.Parameters.AddWithValue(value.PropertyName, value.GetFrom(parameter) ?? DBNull.Value);
        }

        internal static int Execute(this SqlConnection connection, string sql, object parameter)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                AddParameters(command, sql, parameter);
                return command.ExecuteNonQuery();
            }
        }

        internal static async Task<int> ExecuteAsync(this SqlConnection connection, string sql, object parameter)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                AddParameters(command, sql, parameter);
                return await command.ExecuteNonQueryAsync();
            }
        }

        internal static readonly Dictionary<Type, Dictionary<string, Column>> columnCache = new Dictionary<Type, Dictionary<string, Column>>();

        internal static void CheckColumnCache<T>()
        {
            if (!columnCache.ContainsKey(typeof(T)))            
                columnCache[typeof(T)] = typeof(T).GetColumns().ToDictionary(c => c.ColumnName, c => c);            
        }

        internal static IEnumerable<T> Read<T>(this SqlConnection connection, string sql, object parameter)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                CheckColumnCache<T>();
                var columns = columnCache[typeof(T)];
                var provider = new DefaultTypeProvider(); // TODO SqlServerTypeProvider
                AddParameters(command, sql, parameter);
                using (var reader = command.ExecuteReader())
                {
                    reader.GetSchemaTable();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var row = default(T);
                            for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                            {
                                var column = columns[reader.GetName(ordinal)];                                
                                column.SetOn(row, reader.ReadColumn(column, ordinal, provider));
                            }
                            yield return row;
                        }
                    }
                }
            }
        }
    }
}

using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Ooorm.Data
{
    /// <summary>
    /// Core behavior for reading and writing to a database with SQL
    /// </summary>
    /// <typeparam name="TDbConnection">Specific connection type (ex: SqlConnection or SqlLiteConnection)</typeparam>
    /// <typeparam name="TDbCommand">Specific command type (ex: SqlCommand or SqlLiteCommand)</typeparam>
    public abstract class BaseDao<TDbConnection, TDbCommand, TDbReader> where TDbConnection : IDbConnection where TDbCommand : IDbCommand where TDbReader : IDataReader
    {
        protected readonly IDataConsumer<TDbReader> consumer;
        protected readonly ITypeProvider types;

        protected BaseDao(IDataConsumer<TDbReader> consumer, ITypeProvider types)
        {
            this.consumer = consumer;
            this.types = types;
        }

        public abstract TDbCommand GetCommand(string sql, TDbConnection connection);
        public abstract void AddKeyValuePair(TDbCommand command, string key, object value);

        protected static readonly Dictionary<Type, List<Property>> propertyCache = new Dictionary<Type, List<Property>>();

        public void AddParameters(TDbCommand command, string sql, object parameter)
        {
            if (parameter == null)
                return;
            var paramType = parameter.GetType();
            if (!propertyCache.ContainsKey(paramType))
                propertyCache[paramType] = paramType.GetDataProperties().ToList();                            
            foreach (var value in propertyCache[paramType].Where(p => sql.Contains($"@{p.PropertyName}")))
                AddKeyValuePair(command, value.PropertyName, value.GetFrom(parameter) ?? DBNull.Value);
        }

        public virtual int Execute(TDbConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {
                command.CommandText = sql;
                AddParameters(command, sql, parameter);
                return command.ExecuteNonQuery();
            }
        }

        protected static readonly Dictionary<Type, Dictionary<string, Column>> columnCache = new Dictionary<Type, Dictionary<string, Column>>();

        public static void CheckColumnCache<T>()
        {
            if (!columnCache.ContainsKey(typeof(T)))            
                columnCache[typeof(T)] = typeof(T).GetColumns().ToDictionary(c => c.ColumnName, c => c);            
        }

        public virtual IEnumerable<T> Read<T>(TDbConnection connection, string sql, object parameter)
        {
            using (var command = GetCommand(sql, connection))
            {                
                CheckColumnCache<T>();                
                AddParameters(command, sql, parameter);
                return ExecuteReader<T>(command);                     
            }
        }

        protected virtual IEnumerable<T> ExecuteReader<T>(TDbCommand command)
        {            
            using (var reader = (TDbReader)command.ExecuteReader())
            {
                return ParseReader<T>(reader);
            }
        }

        protected virtual IEnumerable<T> ParseReader<T>(TDbReader reader)
        {
            while (reader.Read())
            {
                var row = default(T);
                for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                {
                    var column = columnCache[typeof(T)][reader.GetName(ordinal)];
                    column.SetOn(row, consumer.ReadColumn(reader, column, ordinal, types));
                }
                yield return row;
            }
        }
    }
}

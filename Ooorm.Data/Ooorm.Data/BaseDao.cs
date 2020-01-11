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
    internal abstract class BaseDao<TDbConnection, TDbCommand, TDbReader> where TDbConnection : IDbConnection where TDbCommand : IDbCommand where TDbReader : IDataReader
    {
        protected readonly IDataConsumer<TDbReader> consumer;
        protected readonly IExtendableTypeResolver types;
        protected readonly Func<IDatabase> db;

        protected BaseDao(IDataConsumer<TDbReader> consumer, IExtendableTypeResolver types, Func<IDatabase> db)
        {
            this.consumer = consumer;
            this.types = types;
            this.db = db;
        }

        public abstract TDbCommand GetCommand(string sql, TDbConnection connection);
        public abstract void AddKeyValuePair(TDbCommand command, string key, object value);

        protected static readonly object propertyCacheLock = new object();
        protected static readonly Dictionary<Type, List<Property>> propertyCache = new Dictionary<Type, List<Property>>();

        public void AddParameters(TDbCommand command, string sql, object parameter)
        {
            if (parameter == null)
                return;
            var paramType = parameter.GetType();
            lock (propertyCacheLock)
            {
                if (!propertyCache.ContainsKey(paramType))
                    propertyCache[paramType] = paramType.GetDataProperties().ToList();
            }
            foreach (var value in propertyCache[paramType].Where(p => sql.Contains($"@{p.PropertyName}")))
                AddKeyValuePair(command, value.PropertyName, types.DbSerialize(value.PropertyType, value.GetFrom(parameter)) ?? DBNull.Value);
        }

        public void AddParameters(TDbCommand command, string sql, (string name, object value) parameter)
        {
            if (types.IsDbValueType(parameter.value.GetType()))
                AddKeyValuePair(command, parameter.name, types.DbSerialize(parameter.value.GetType(), parameter.value) ?? DBNull.Value);
            else
                AddParameters(command, sql, parameter.value);
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

        protected static readonly object columnCacheLock = new object();
        protected static readonly Dictionary<Type, Dictionary<string, Column>> columnCache = new Dictionary<Type, Dictionary<string, Column>>();

        public static void CheckColumnCache<T, TId>() where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            lock (columnCacheLock)
            {
                if (!columnCache.ContainsKey(typeof(T)))
                    columnCache[typeof(T)] = typeof(T).GetColumns().ToDictionary(c => c.ColumnName, c => c);
            }
        }

        public virtual List<T> Read<T, TId>(TDbConnection connection, string sql, object parameter) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            CheckColumnCache<T, TId>();
            using (var command = GetCommand(sql, connection))
            {                
                AddParameters(command, sql, parameter);
                return ExecuteReader<T, TId>(command);
            }
        }

        protected virtual List<T> ExecuteReader<T, TId>(TDbCommand command) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            using (var reader = (TDbReader)command.ExecuteReader())
            {
                return ParseReader<T, TId>(reader);
            }
        }

        protected virtual List<T> ParseReader<T, TId>(TDbReader reader) where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>
        {            
            var results = new List<T>(reader.RecordsAffected /* estimate of number of results */);
            while (reader.Read())
            {
                var row = typeof(T).IsValueType ? default : Activator.CreateInstance<T>();
                for (int ordinal = 0; ordinal < reader.FieldCount; ordinal++)
                {
                    var column = columnCache[typeof(T)][reader.GetName(ordinal)];
                    var columnValue = consumer.ReadColumn(reader, column, ordinal, types);
                    column.SetOn(row, columnValue);
                }
                results.Add(row);
            }
            return results;
        }
    }
}

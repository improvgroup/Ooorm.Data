using Ooorm.Data.Core.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Ooorm.Data.Core
{
    internal abstract class BaseDataConsumer<TDataReader> : IDataConsumer<TDataReader> where TDataReader : IDataReader
    {
        protected abstract bool TryGetStream(TDataReader reader, int ordinal, out Stream stream);

        public virtual IEnumerable<string> Fields(TDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                yield return reader.GetName(i);
        }

        private static MethodInfo dateTimeOffsetMethodCache;

        public virtual object ReadColumn(TDataReader reader, Column column, int index, IExtendableTypeResolver types)
        {
            return types.DbDeserialize(column.PropertyType, ReadColumnFromReader(reader, column, index, types));
        }

        private object ReadColumnFromReader(TDataReader reader, Column column, int index, IExtendableTypeResolver types)
        {
            if (reader.IsDBNull(index))
                return null;
            switch (types.GetDbType(column))
            {
                case DbType.Boolean:
                    return reader.GetBoolean(index);
                case DbType.Byte:
                    return reader.GetByte(index);
                case DbType.SByte:
                    return (sbyte)reader.GetByte(index);
                case DbType.Int16:
                    return reader.GetInt16(index);
                case DbType.UInt16:
                    return (ushort)reader.GetInt16(index);
                case DbType.Int32:
                    return reader.GetInt32(index);
                case DbType.UInt32:
                    return (uint)reader.GetInt32(index);
                case DbType.Int64:
                    return reader.GetInt64(index);
                case DbType.UInt64:
                    return (ulong)reader.GetInt64(index);
                case DbType.Single:
                    return reader.GetFloat(index);
                case DbType.Double:
                    return reader.GetDouble(index);
                case DbType.Decimal:
                case DbType.Currency:
                    return reader.GetDecimal(index);
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return ReadStringField(reader, column, index);
                case DbType.Guid:
                    return reader.GetGuid(index);
                case DbType.DateTime:
                case DbType.DateTime2:
                    return reader.IsDBNull(index) ? default : reader.GetDateTime(index);
                case DbType.DateTimeOffset:
                    if (dateTimeOffsetMethodCache == null)
                    {
                        dateTimeOffsetMethodCache =
                            reader.GetType()
                                  .GetMethods()
                                  .Where(m => m.ReturnType == typeof(DateTimeOffset))
                                  .FirstOrDefault(m =>
                                  {
                                      var p = m.GetParameters();
                                      if (p.Count() == 1 && p.First().ParameterType == typeof(int))
                                          return true;
                                      return false;
                                  });                                                   
                    }
                    return (DateTimeOffset)dateTimeOffsetMethodCache.Invoke(reader, new object[] { index });
                case DbType.Binary:
                    return ReadBinaryField(reader, column, index);
                default:
                    return null;
            }
        }

        protected virtual object ReadStringField(TDataReader reader, Column column, int index)
        {
            string data = null;
            if (!reader.IsDBNull(index))
                data = reader.GetString(index);            
            return data;
        }

        protected virtual object ReadBinaryField(TDataReader reader, Column column, int index)
        {            
            var buffer = new byte[reader.GetBytes(index, 0, null, 0, 0)];
            reader.GetBytes(index, 0, buffer, 0, buffer.Length);
            return buffer;            
        }
    }
}

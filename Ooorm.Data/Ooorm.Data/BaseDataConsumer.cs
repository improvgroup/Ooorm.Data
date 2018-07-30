using Newtonsoft.Json;
using Ooorm.Data.Attributes;
using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;

namespace Ooorm.Data
{
    public abstract class BaseDataConsumer<TDataReader> : IDataConsumer<TDataReader> where TDataReader : IDataReader
    {
        protected abstract bool TryGetStream(TDataReader reader, int ordinal, out Stream stream);
        
        public virtual IEnumerable<string> Fields(TDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                yield return reader.GetName(i);
        }

        public virtual object ReadColumn(TDataReader reader, Column column, int index, ITypeProvider types)
        {
            switch (types.DbType(column.PropertyType))
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
                case DbType.Binary:                                                            
                    return ReadBinaryField(reader, column, index);
                default:
                    return null;
            }
        }

        protected virtual object ReadStringField(TDataReader reader, Column column, int index)
        {
            var data = reader.GetString(index);
            if (column.PropertyType == typeof(string))
                return data;
            else if (column.PropertyType == typeof(char[]))
                return data.ToCharArray();
            else if (column.PropertyType == typeof(XmlDocument))
            {
                var doc = new XmlDocument();
                doc.Load(data);
                return doc;
            }
            else if (column.Info.HasAttribute<AsJsonAttribute>())
                return JsonConvert.DeserializeObject(data, column.PropertyType);
            else if (column.Info.HasAttribute<AsXmlAttribute>())                            
                using (TextReader text = new StringReader(data))
                    return new XmlSerializer(column.PropertyType).Deserialize(text);
            throw new NotSupportedException($"Can not convert DbString type to {column.PropertyType} for column {column.ColumnName} -> {column.PropertyName}");
        }

        protected virtual object ReadBinaryField(TDataReader reader, Column column, int index)
        {
            if (column.PropertyType == typeof(byte[]))
            {
                var buffer = new byte[reader.GetBytes(index, 0, null, 0, 0)];
                reader.GetBytes(index, 0, buffer, 0, buffer.Length);
                return buffer;
            }
            else if (column.Info.HasAttribute<AsBinaryAttribute>() && TryGetStream(reader, index, out Stream stream))
                return new BinaryFormatter().Deserialize(stream);
            throw new NotSupportedException($"Can not convert DbString type to {column.PropertyType} for column {column.ColumnName} -> {column.PropertyName}");
        }        
    }
}

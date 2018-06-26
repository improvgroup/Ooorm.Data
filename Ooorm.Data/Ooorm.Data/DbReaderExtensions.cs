using Newtonsoft.Json;
using Ooorm.Data.Attributes;
using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ooorm.Data
{
    public static class DbReaderExtensions
    {
        public static IEnumerable<string> Fields(this System.Data.Common.DbDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                yield return reader.GetName(i);
        }

        public static object ReadColumn(this System.Data.Common.DbDataReader reader, Column column, int index, ITypeProvider types)
        {
            switch (types.DbType(column.PropertyType))
            {
                case System.Data.DbType.Boolean:
                    return reader.GetBoolean(index);
                case System.Data.DbType.Byte:
                    return reader.GetByte(index);
                case System.Data.DbType.SByte:
                    return (sbyte)reader.GetByte(index);
                case System.Data.DbType.Int16:
                    return reader.GetInt16(index);
                case System.Data.DbType.UInt16:
                    return (ushort)reader.GetInt16(index);
                case System.Data.DbType.Int32:
                    return reader.GetInt32(index);
                case System.Data.DbType.UInt32:
                    return (uint)reader.GetInt32(index);
                case System.Data.DbType.Int64:
                    return reader.GetInt64(index);
                case System.Data.DbType.UInt64:
                    return (ulong)reader.GetInt64(index);
                case System.Data.DbType.Single:
                    return reader.GetFloat(index);
                case System.Data.DbType.Double:
                    return reader.GetDouble(index);
                case System.Data.DbType.Decimal:                    
                case System.Data.DbType.Currency:
                    return reader.GetDecimal(index);
                case System.Data.DbType.AnsiString:                    
                case System.Data.DbType.AnsiStringFixedLength:                    
                case System.Data.DbType.String:                    
                case System.Data.DbType.StringFixedLength:
                    return ReadStringField(reader, column, index);
                case System.Data.DbType.Guid:
                    return reader.GetGuid(index);
                case System.Data.DbType.DateTime:
                case System.Data.DbType.DateTime2:
                    return reader.IsDBNull(index) ? default : reader.GetDateTime(index);                
                case System.Data.DbType.Binary:                                                            
                    return ReadBinaryField(reader, column, index);
                default:
                    return null;
            }
        }

        private static object ReadStringField(this System.Data.Common.DbDataReader reader, Column column, int index)
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

        private static object ReadBinaryField(this System.Data.Common.DbDataReader reader, Column column, int index)
        {
            if (column.PropertyType == typeof(byte[]))
            {
                var buffer = new byte[reader.GetBytes(index, 0, null, 0, 0)];
                reader.GetBytes(index, 0, buffer, 0, buffer.Length);
                return buffer;
            }
            else if (column.Info.HasAttribute<AsBinaryAttribute>())
                return new BinaryFormatter().Deserialize(reader.GetStream(index));       
            throw new NotSupportedException($"Can not convert DbString type to {column.PropertyType} for column {column.ColumnName} -> {column.PropertyName}");
        }
    }
}

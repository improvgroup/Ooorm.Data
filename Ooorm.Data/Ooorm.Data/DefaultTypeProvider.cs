using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using Ooorm.Data.Reflection;

namespace Ooorm.Data
{
    internal class DefaultTypeProvider : ITypeProvider
    {
        private readonly Func<IDatabase> database;

        public DefaultTypeProvider(Func<IDatabase> db) => database = db;

        public Type ClrType(DbType dbType)
        {
            switch (dbType)
            {
                case System.Data.DbType.Boolean:
                    return typeof(bool);
                case System.Data.DbType.Byte:
                    return typeof(byte);
                case System.Data.DbType.SByte:
                    return typeof(sbyte);
                case System.Data.DbType.Int16:
                    return typeof(short);
                case System.Data.DbType.UInt16:
                    return typeof(ushort);
                case System.Data.DbType.Int32:
                    return typeof(int);
                case System.Data.DbType.UInt32:
                    return typeof(uint);
                case System.Data.DbType.Int64:
                    return typeof(long);
                case System.Data.DbType.UInt64:
                    return typeof(ulong);
                case System.Data.DbType.Single:
                    return typeof(float);
                case System.Data.DbType.Double:
                    return typeof(double);
                case System.Data.DbType.Decimal:
                    return typeof(decimal);
                case System.Data.DbType.Currency:
                    return typeof(decimal);
                case System.Data.DbType.AnsiString:
                    return typeof(char[]);
                case System.Data.DbType.AnsiStringFixedLength:
                    return typeof(char[]);
                case System.Data.DbType.String:
                    return typeof(string);
                case System.Data.DbType.StringFixedLength:
                    return typeof(string);
                case System.Data.DbType.Guid:
                    return typeof(Guid);
                case System.Data.DbType.DateTime:
                case System.Data.DbType.DateTime2:
                    return typeof(DateTime);
                case System.Data.DbType.DateTimeOffset:
                    return typeof(DateTimeOffset);
                case System.Data.DbType.Time:
                    return typeof(TimeSpan);
                case System.Data.DbType.Binary:
                    return typeof(byte[]);
                case System.Data.DbType.Xml:
                    return typeof(XmlDocument);
                default:
                    return null;
            }
        }

        private static readonly IReadOnlyDictionary<Type, DbType> _typeMap = new Dictionary<Type, DbType>
        {
            { typeof(bool),           System.Data.DbType.Boolean        },
            { typeof(byte),           System.Data.DbType.Byte           },
            { typeof(sbyte),          System.Data.DbType.SByte          },
            { typeof(short),          System.Data.DbType.Int16          },
            { typeof(ushort),         System.Data.DbType.UInt16         },
            { typeof(int),            System.Data.DbType.Int32          },
            { typeof(int?),           System.Data.DbType.Int32          },
            { typeof(DbVal<>),        System.Data.DbType.Int32          },
            { typeof(DbRef<>),        System.Data.DbType.Int32          },
            { typeof(uint),           System.Data.DbType.UInt32         },
            { typeof(long),           System.Data.DbType.Int64          },
            { typeof(ulong),          System.Data.DbType.UInt64         },
            { typeof(float),          System.Data.DbType.Single         },
            { typeof(double),         System.Data.DbType.Double         },
            { typeof(decimal),        System.Data.DbType.Decimal        },
            { typeof(char[]),         System.Data.DbType.AnsiString     },
            { typeof(string),         System.Data.DbType.String         },
            { typeof(Guid),           System.Data.DbType.Guid           },
            { typeof(DateTime),       System.Data.DbType.DateTime       },
            { typeof(DateTimeOffset), System.Data.DbType.DateTimeOffset },
            { typeof(TimeSpan),       System.Data.DbType.Time           },
            { typeof(byte[]),         System.Data.DbType.Binary         },
            { typeof(XmlDocument),    System.Data.DbType.Xml            },
        };

        public DbType DbType<TClrType>() => DbType(typeof(TClrType));

        public DbType DbType(Type clrType)
        {
            if (_typeMap.ContainsKey(clrType))
                return _typeMap[clrType];
            else if (IsNullable(clrType, out Type generic) && _typeMap.ContainsKey(generic))
                return _typeMap[generic];
            else if (IsDbVal(clrType) || IsDbRef(clrType))
                return System.Data.DbType.Int32;
            else if (clrType.IsEnum)
                return DbType(clrType.GetEnumUnderlyingType());
            else
                throw new InvalidOperationException($"Cannot translate clr type {clrType} to a database type");
        }

        private bool IsNullable(Type clrType, out Type generic)
            => (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>)) ? (generic = clrType.GenericTypeArguments.FirstOrDefault()) != null : (generic = null) != null;

        private bool IsDbVal(Type clrType)
            => clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(DbVal<>);

        private bool IsDbRef(Type clrType)
            => clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(DbRef<>);

        public bool IsDbValueType(Type clrType)
            => (clrType.IsGenericType && (clrType == typeof(DbRef<>).MakeGenericType(clrType.GenericTypeArguments) || clrType == typeof(DbVal<>).MakeGenericType(clrType.GenericTypeArguments)))
                    || _typeMap.ContainsKey(clrType);

        public string DbTypeString(Column column)
        {
            switch (DbType(column.PropertyType))
            {
                case System.Data.DbType.Boolean:
                    return "bit";
                case System.Data.DbType.Byte:
                case System.Data.DbType.SByte:
                    return "tinyint";
                case System.Data.DbType.Int16:
                case System.Data.DbType.UInt16:
                    return "smallint";
                case System.Data.DbType.Int32:
                case System.Data.DbType.UInt32:
                    return "int";
                case System.Data.DbType.Int64:
                case System.Data.DbType.UInt64:
                    return "bigint";
                case System.Data.DbType.Single:
                    return "real";
                case System.Data.DbType.Double:
                    return "float";
                case System.Data.DbType.Decimal:
                case System.Data.DbType.Currency:
                    return "decimal";
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.AnsiStringFixedLength:
                    if (column.Info.TryGetAttribute(out FixedLengthAttribute fixedLengthAnsi))
                        return $"char({fixedLengthAnsi.Length})";
                    else if (column.Info.TryGetAttribute(out MaxLengthAttribute maxLengthAnsi))
                        return $"varchar({maxLengthAnsi.Length})";
                    else
                        return "varchar(max)";
                case System.Data.DbType.String:
                case System.Data.DbType.StringFixedLength:
                    if (column.Info.TryGetAttribute(out FixedLengthAttribute fixedLengthString))
                        return $"nchar({fixedLengthString.Length})";
                    else if (column.Info.TryGetAttribute(out MaxLengthAttribute maxLengthString))
                        return $"nvarchar({maxLengthString.Length})";
                    else
                        return "nvarchar(max)";
                case System.Data.DbType.Guid:
                    return "uniqueidentifier";
                case System.Data.DbType.DateTime:
                    return "datetime";
                case System.Data.DbType.DateTime2:
                    return "datetime2";
                case System.Data.DbType.DateTimeOffset:
                    return "datetimeoffset";
                case System.Data.DbType.Time:
                    return "time";
                case System.Data.DbType.Binary:
                    if (column.Info.TryGetAttribute(out FixedLengthAttribute fixedLengthBinary))
                        return $"binary({fixedLengthBinary})";
                    else if (column.Info.TryGetAttribute(out MaxLengthAttribute maxLengthBinary))
                        return $"varbinary({maxLengthBinary})";
                    else
                        return $"varbinary(max)";
                case System.Data.DbType.Xml:
                    return "nvarchar(max)";
                default:
                    return null;
            }
        }

        public object ToDbValue(object value)
        {
            if (value == null)
                return DBNull.Value;
            else if (value.GetType().IsEnum)
                return (int)value;
            else if (value is IdConvertable<int> valId)
                return valId.ToId();
            else if (value is IdConvertable<int?> refId)
                return refId.ToId();
            return value;
        }

        public object FromDbValue(object value, Type type)
        {
            if (value == DBNull.Value)
                return null;
            if (type.IsEnum)
                return Enum.ToObject(type, value);
            if (IsDbVal(type))
                return Activator.CreateInstance(
                    typeof(DbVal<>).MakeGenericType(type.GenericTypeArguments), value, database);
            else if (IsDbRef(type))
                return Activator.CreateInstance(
                    typeof(DbRef<>).MakeGenericType(type.GenericTypeArguments), value, database);
            return value;
        }
    }
}


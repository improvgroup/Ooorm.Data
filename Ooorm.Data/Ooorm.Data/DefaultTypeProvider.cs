using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using Ooorm.Data.Attributes;
using Ooorm.Data.Reflection;

namespace Ooorm.Data
{
    public class DefaultTypeProvider : ITypeProvider
    {
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
            { typeof(bool?),          System.Data.DbType.Boolean        },
            { typeof(byte),           System.Data.DbType.Byte           },
            { typeof(byte?),          System.Data.DbType.Byte           },
            { typeof(sbyte),          System.Data.DbType.SByte          },
            { typeof(sbyte?),         System.Data.DbType.SByte          },
            { typeof(short),          System.Data.DbType.Int16          },
            { typeof(short?),         System.Data.DbType.Int16          },
            { typeof(ushort),         System.Data.DbType.UInt16         },
            { typeof(ushort?),        System.Data.DbType.UInt16         },
            { typeof(int),            System.Data.DbType.Int32          },
            { typeof(int?),           System.Data.DbType.Int32          },
            { typeof(uint),           System.Data.DbType.UInt32         },
            { typeof(uint?),          System.Data.DbType.UInt32         },
            { typeof(long),           System.Data.DbType.Int64          },
            { typeof(long?),          System.Data.DbType.Int64          },
            { typeof(ulong),          System.Data.DbType.UInt64         },
            { typeof(ulong?),         System.Data.DbType.UInt64         },
            { typeof(float),          System.Data.DbType.Single         },
            { typeof(float?),         System.Data.DbType.Single         },
            { typeof(double),         System.Data.DbType.Double         },
            { typeof(double?),        System.Data.DbType.Double         },
            { typeof(decimal),        System.Data.DbType.Decimal        },
            { typeof(decimal?),       System.Data.DbType.Decimal        },
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
        
        public DbType DbType(Type clrType) => _typeMap[clrType];                

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
    }
}

                
using System;
using System.Data;
using Ooorm.Data.Reflection;
using Ooorm.Data.TypeResolvers;

namespace Ooorm.Data.Sqlite
{
    internal class SqliteDateTimeOffsetHandler : TypeHandler<DateTimeOffset, string>
    {
        public override DbType GetDbType(Column column) => DbType.String;

        public override string GetDbTypeString(Column column) => "NVARCHAR(128)";

        public override string Serialize(DateTimeOffset value) => $"{value.ToString()}|{value.DateTime.ToBinary()}|{value.Offset.Ticks}";

        public override DateTimeOffset Deserialize(string value)
        {
            var data = value.Split('|');
            return new DateTimeOffset(DateTime.FromBinary(long.Parse(data[1])), TimeSpan.FromTicks(long.Parse(data[2])));
        }
    }

    internal class SqliteBooleanHandler : BooleanHandler
    {        
        public override string GetDbTypeString(Column column) => "BOOLEAN";        
    }

    internal class SqliteBinaryHandler : ByteArrayHandler
    {
        public override string GetDbTypeString(Column column) => "BLOB";
    }

    internal class SqliteStringHandler : StringHandler
    {
        public override string GetDbTypeString(Column column)
        {
            if (column.Info.TryGetAttribute(out FixedLengthAttribute f))
                return $"NCHAR({f.Length})";
            else if (column.Info.TryGetAttribute(out MaxLengthAttribute m))
                return $"NVARCHAR({m.Length})";
            else
                return $"TEXT";
        }
    }

    internal class SqliteTypeProvider : ExtendableTypeProvider
    {
        internal SqliteTypeProvider(Func<IDatabase> db) : base(db)
        {
            RegisterHandler(new SqliteDateTimeOffsetHandler());
            RegisterHandler(new SqliteBooleanHandler());
            RegisterHandler(new SqliteBinaryHandler());
            RegisterHandler(new SqliteStringHandler());
        }
    }   
}

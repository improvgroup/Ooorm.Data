using Ooorm.Data.Core.Reflection;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public class StringHandler : TypeHandler<string, string>
    {
        public override DbType GetDbType(Column column)
        {
            if (column.Info.TryGetAttribute(out FixedLengthAttribute f))
                return DbType.StringFixedLength;
            else
                return DbType.String;
        }

        public override string GetDbTypeString(Column column)
        {
            if (column.Info.TryGetAttribute(out FixedLengthAttribute f))
                return $"NCHAR({f.Length})";
            else if (column.Info.TryGetAttribute(out MaxLengthAttribute m))
                return $"NVARCHAR({m.Length})";
            else
                return $"NVARCHAR(MAX)";
        }

        public override string Deserialize(string value) => value;

        public override string Serialize(string value) => value;
    }
}

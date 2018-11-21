using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class Int8Handler : TypeHandler<sbyte, sbyte>
    {
        public override DbType GetDbType(Column column) => DbType.SByte;

        public override string GetDbTypeString(Column column) => "TINYINT";

        public override sbyte Deserialize(sbyte value) => value;

        public override sbyte Serialize(sbyte value) => value;
    }
}

using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class UInt64Handler : TypeHandler<ulong, ulong>
    {
        public override DbType GetDbType(Column column) => DbType.UInt64;

        public override string GetDbTypeString(Column column) => "BIGINT";

        public override ulong Deserialize(ulong value) => value;

        public override ulong Serialize(ulong value) => value;
    }
}

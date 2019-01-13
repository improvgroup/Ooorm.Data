using Ooorm.Data.Core.Reflection;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public class UInt16Handler : TypeHandler<ushort, ushort>
    {
        public override DbType GetDbType(Column column) => DbType.UInt16;

        public override string GetDbTypeString(Column column) => "SMALLINT";

        public override ushort Deserialize(ushort value) => value;

        public override ushort Serialize(ushort value) => value;
    }
}

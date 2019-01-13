using Ooorm.Data.Core.Reflection;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public class UInt32Handler : TypeHandler<uint, uint>
    {
        public override DbType GetDbType(Column column) => DbType.UInt32;

        public override string GetDbTypeString(Column column) => "INT";

        public override uint Deserialize(uint value) => value;

        public override uint Serialize(uint value) => value;
    }
}

using Ooorm.Data.Core.Reflection;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public class UInt8Handler : TypeHandler<byte, byte>
    {
        public override DbType GetDbType(Column column) => DbType.Byte;

        public override string GetDbTypeString(Column column) => "TINYINT";

        public override byte Deserialize(byte value) => value;

        public override byte Serialize(byte value) => value;
    }
}

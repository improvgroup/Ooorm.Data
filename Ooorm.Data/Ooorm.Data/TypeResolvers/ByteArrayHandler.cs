using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class ByteArrayHandler : TypeHandler<byte[], byte[]>
    {
        public override DbType GetDbType(Column column) => DbType.Binary;

        public override string GetDbTypeString(Column column)
        {
            if (column.Info.TryGetAttribute(out FixedLengthAttribute f))
                return $"BINARY({f.Length})";
            else if (column.Info.TryGetAttribute(out MaxLengthAttribute m))
                return $"VARBINARY({m.Length})";
            else
                return $"VARBINARY(MAX)";
        }

        public override byte[] Deserialize(byte[] value) => value;

        public override byte[] Serialize(byte[] value) => value;
    }
}

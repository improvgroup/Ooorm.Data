using Ooorm.Data.Reflection;
using System;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class GuidHandler : TypeHandler<Guid, byte[]>
    {
        public override DbType GetDbType(Column column) => DbType.Binary;

        public override string GetDbTypeString(Column column) => "BINARY(16)";

        public override Guid Deserialize(byte[] value) => new Guid(value);

        public override byte[] Serialize(Guid value) => value.ToByteArray();
    }
}

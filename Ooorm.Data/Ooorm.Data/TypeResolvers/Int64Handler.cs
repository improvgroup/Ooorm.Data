using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class Int64Handler : TypeHandler<long, long>
    {
        public override DbType GetDbType(Column column) => DbType.Int64;

        public override string GetDbTypeString(Column column) => "BIGINT";

        public override long Deserialize(long value) => value;

        public override long Serialize(long value) => value;
    }
}

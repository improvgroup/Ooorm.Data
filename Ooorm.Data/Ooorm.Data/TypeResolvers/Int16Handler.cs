using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class Int16Handler : TypeHandler<short, short>
    {
        public override DbType GetDbType(Column column) => DbType.Int16;

        public override string GetDbTypeString(Column column) => "SMALLINT";

        public override short Deserialize(short value) => value;

        public override short Serialize(short value) => value;
    }
}

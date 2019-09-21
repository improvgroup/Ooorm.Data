using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class BooleanHandler : TypeHandler<bool, bool>
    {
        public override DbType GetDbType(Column column) => DbType.Boolean;

        public override string GetDbTypeString(Column column) => "BIT";

        public override bool Deserialize(bool value) => value;

        public override bool Serialize(bool value) => value;
    }
}

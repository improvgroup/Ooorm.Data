using Ooorm.Data.Reflection;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class FloatHandler : TypeHandler<float, float>
    {
        public override DbType GetDbType(Column column) => DbType.Single;

        public override string GetDbTypeString(Column column) => "REAL";

        public override float Deserialize(float value) => value;

        public override float Serialize(float value) => value;
    }
}

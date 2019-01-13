using Ooorm.Data.Core.Reflection;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public class Int32Handler : TypeHandler<int, int>
    {
        public override DbType GetDbType(Column column) => DbType.Int32;

        public override string GetDbTypeString(Column column) => "INT";

        public override int Deserialize(int value) => value;

        public override int Serialize(int value) => value;
    }
}

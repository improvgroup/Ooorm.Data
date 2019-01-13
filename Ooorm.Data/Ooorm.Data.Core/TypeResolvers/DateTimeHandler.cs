using Ooorm.Data.Core.Reflection;
using System;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public class DateTimeHandler : TypeHandler<DateTime, DateTime>
    {
        public override DbType GetDbType(Column column) => DbType.DateTime;

        public override string GetDbTypeString(Column column) => "DATETIME";

        public override DateTime Deserialize(DateTime value) => value;

        public override DateTime Serialize(DateTime value) => value;
    }
}

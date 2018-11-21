using Ooorm.Data.Reflection;
using System;
using System.Data;

namespace Ooorm.Data.TypeResolvers
{
    public class DateTimeOffsetHandler : TypeHandler<DateTimeOffset, DateTimeOffset>
    {
        public override DbType GetDbType(Column column) => DbType.DateTimeOffset;

        public override string GetDbTypeString(Column column) => "DATETIMEOFFSET";

        public override DateTimeOffset Deserialize(DateTimeOffset value) => value;

        public override DateTimeOffset Serialize(DateTimeOffset value) => value;
    }


}

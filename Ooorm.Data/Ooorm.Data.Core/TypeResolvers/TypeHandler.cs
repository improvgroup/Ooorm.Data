using Ooorm.Data.Core.Reflection;
using System;
using System.Data;

namespace Ooorm.Data.Core.TypeResolvers
{
    public abstract class TypeHandler
    {
        public abstract DbType GetDbType(Column column);

        public abstract string GetDbTypeString(Column column);

        public abstract object SerializeObject(object value);

        public abstract object DeserializeObject(object value);
    }

    public abstract class TypeHandler<TClr, TDb> : TypeHandler
    {
        public abstract TDb Serialize(TClr value);

        public abstract TClr Deserialize(TDb value);

        public override object SerializeObject(object value) => Serialize((TClr)value);

        public override object DeserializeObject(object value) => Deserialize((TDb)value);
    }    
}

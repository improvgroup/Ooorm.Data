using Ooorm.Data.Attributes;
using Ooorm.Data.Reflection;
using Ooorm.Data.Schemas;
using System;
using System.Linq;

namespace Ooorm.Data.SqlServer
{
    internal class SqlServerSchemaProvider : ISchemaProvider
    {
        public string DatabaseSql(string name)
        {
            return $"USE MASTER; CREATE DATABSE {name};";
        }

        public string TableSql<TModel>(ITypeProvider types) where TModel : IDbItem
        {
            string name = typeof(TModel).Name;
            if (typeof(TModel).TryGetAttribute(out TableAttribute table))
                name = table.Value;
            return 
$@"CREATE TABLE {name} (
    {string.Join($",{Environment.NewLine}", typeof(TModel).GetColumns().Select(c => $"{c.ColumnName} {types.DbTypeString(c)}"))}
);";
        }
    }
}

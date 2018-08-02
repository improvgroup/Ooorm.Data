using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ISchema
    {
        Task DropDatabase(string name);
        Task CreateDatabase(string name, params Type[] tables);
        Task CreateTable<T>() where T : IDbItem;
        Task CreateTables(params Type[] tables);
    }

    internal interface ISchemaProvider
    {
        string DropDatabaseSql(string name);
        string DatabaseSql(string name);
    }
}

using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ISchema
    {        
        Task CreateTable<T>() where T : IDbItem;
        Task CreateTables(params Type[] tables);
        Task DropTable<T>() where T : IDbItem;
        Task DropTables(params Type[] tables);
    }

    internal interface IDatabaseManagementQueryProvider
    {
        string DropDatabaseSql(string name);
        string DatabaseSql(string name);
    }
}

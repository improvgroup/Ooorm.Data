using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface ISchema
    {        
        Task CreateTable<T, TId>() where T : IDbItem<TId> where TId : struct, IEquatable<TId>;
        Task CreateTables(params Type[] tables);
        Task DropTable<T, TId>() where T : IDbItem<TId> where TId : struct, IEquatable<TId>;
        Task DropTables(params Type[] tables);
    }

    internal interface IDatabaseManagementQueryProvider
    {
        string DropDatabaseSql(string name);
        string DatabaseSql(string name);
    }
}

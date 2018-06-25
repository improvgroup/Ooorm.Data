using System.Data;
using System.IO;

namespace Ooorm.Data.Schemas
{
    public interface ISchemaProvider<TDbConnection> : IConnectionDependent<TDbConnection> where TDbConnection : IDbConnection
    {
        string DatabaseSql();
        string RestoreFromBackupSql(FileInfo backup);
        string TableSql<TModel>(ITypeProvider types) where TModel : IDbItem;
        string AlterTableSql<TModel>(dynamic options) where TModel : IDbItem;
        string AddUserSql(string user, dynamic options);
        string AlterUserSql(string user, dynamic options);
    }
}

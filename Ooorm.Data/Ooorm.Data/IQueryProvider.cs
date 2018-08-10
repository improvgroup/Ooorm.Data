using System;
using System.Linq.Expressions;

namespace Ooorm.Data.QueryProviders
{
    internal interface IQueryProvider<T> where T : IDbItem
    {
        string WhereClause<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        string WhereClause(Expression<Func<T, bool>> predicate);
        string WriteSql();
        string UpdateSql<TParam>();
        string DeleteSqlById();
        string DeleteSql(Expression<Func<T, bool>> predicate);
        string DeleteSql<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        string ReadSql();
        string ReadSqlById();
        string ReadSql(Expression<Func<T, bool>> predicate);
        string ReadSql<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param);
        string CreateTableSql();
        string DropTableSql();
    }
}

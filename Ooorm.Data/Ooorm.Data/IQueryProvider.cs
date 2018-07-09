using System;
using System.Linq.Expressions;

namespace Ooorm.Data.QueryProviders
{
    public interface IQueryProvider<T> where T : IDbItem
    {
        string WhereClause<TParam>(Expression<Func<T, TParam, bool>> predicate);
        string WhereClause(Expression<Func<T, bool>> predicate);
        string CreateSql();
        string UpdateSql<TParam>();
        string DeleteSqlById();
        string DeleteSql(Expression<Func<T, bool>> predicate);
        string DeleteSql<TParam>(Expression<Func<T, TParam, bool>> predicate);
        string ReadSql();
        string ReadSqlById();
        string ReadSql(Expression<Func<T, bool>> predicate);
        string ReadSql<TParam>(Expression<Func<T, TParam, bool>> predicate);
    }
}

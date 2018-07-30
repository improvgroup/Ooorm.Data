using System;
using System.Linq.Expressions;

namespace Ooorm.Data.QueryProviders
{
    public class SqliteQueryProvider<T> : IQueryProvider<T> where T : IDbItem
    {
        public string CreateTableSql()
        {
            throw new NotImplementedException();
        }

        public string DeleteSql(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string DeleteSql<TParam>(Expression<Func<T, TParam, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string DeleteSqlById()
        {
            throw new NotImplementedException();
        }

        public string DropTableSql()
        {
            throw new NotImplementedException();
        }

        public string ReadSql()
        {
            throw new NotImplementedException();
        }

        public string ReadSql(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string ReadSql<TParam>(Expression<Func<T, TParam, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string ReadSqlById()
        {
            throw new NotImplementedException();
        }

        public string UpdateSql<TParam>()
        {
            throw new NotImplementedException();
        }

        public string WhereClause<TParam>(Expression<Func<T, TParam, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string WhereClause(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string WriteSql()
        {
            throw new NotImplementedException();
        }
    }
}

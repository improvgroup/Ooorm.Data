using System;
using System.Linq.Expressions;

namespace Ooorm.Data.QueryProviders
{
    public class SqlServerQueryProvider<T> : IQueryProvider<T> where T : IDbItem
    {
        public string DeleteSql<TParam>(int id)
        {
            throw new NotImplementedException();
        }

        public string DeleteSql(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string DeleteSql<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string InsertSql<TParam>(TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string ReadSql(int id)
        {
            throw new NotImplementedException();
        }

        public string ReadSql(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public string ReadSql<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string UpdateSql<TParam>(int id, TParam parameters)
        {
            throw new NotImplementedException();
        }

        public string WhereClause<TParam>(Expression<Func<T, TParam, bool>> predicate)
        {
            return $"WHERE {predicate.ToSql()}";
        }

        public string WhereClause(Expression<Func<T, bool>> predicate)
        {
            return $"WHERE {predicate.ToSql()}";
        }        
    }
}

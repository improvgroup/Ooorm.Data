using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Ooorm.Data.QueryProviders
{
    public class SqliteQueryProvider<T> : IQueryProvider<T> where T : IDbItem
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
            throw new NotImplementedException();
        }

        public string WhereClause(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}

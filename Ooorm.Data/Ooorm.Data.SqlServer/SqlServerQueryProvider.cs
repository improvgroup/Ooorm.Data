using Ooorm.Data.Attributes;
using Ooorm.Data.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ooorm.Data.QueryProviders;

namespace Ooorm.Data.SqlServer
{
    internal class SqlServerQueryProvider<T> : IQueryProvider<T> where T : IDbItem
    {
        private static readonly Column[] COLUMNS = typeof(T).GetColumns().ToArray();
        private static readonly Column[] NON_ID_COLUMNS = typeof(T).GetColumns(exceptId: true).ToArray();
        private static readonly Column ID_COLUMN = COLUMNS.Single(c => c.Info.HasAttribute<IdAttribute>());
        private static readonly string TABLE = typeof(T).HasAttribute<TableAttribute>() ? $"[{typeof(T).GetCustomAttribute<TableAttribute>().Value}]" : $"[{typeof(T).Name}]";
        private static readonly string WHERE_ID = "WHERE [ID] = @ID;";
        private static readonly string DELETE_PREFIX = $"DELETE FROM {TABLE} ";
        private static readonly string DELETE_WHERE_ID = DELETE_PREFIX + WHERE_ID;
        private static readonly string UPDATE_PREFIX = $"UPDATE {TABLE} SET ";
        private static readonly string READ_PREFIX = $"SELECT {string.Join(", ", COLUMNS.Select(c => $"[{c.ColumnName}]"))} FROM {TABLE} ";
        private static readonly string READ_WHERE_ID = READ_PREFIX + WHERE_ID;
        private static readonly string CREATE_SQL = $"INSERT INTO {TABLE} ({string.Join(", ", NON_ID_COLUMNS.Select(c => $"[{c.ColumnName}]"))}) VALUES ({string.Join(", ", NON_ID_COLUMNS.Select(c => $"@{c.ColumnName}"))});";               

        public string DeleteSqlById()
            => DELETE_WHERE_ID;

        public string DeleteSql(Expression<Func<T, bool>> predicate)
            => DELETE_PREFIX.Append(WhereClause(predicate)).Append(";").ToString();

        public string DeleteSql<TParam>(Expression<Func<T, TParam, bool>> predicate)
            => DELETE_PREFIX.Append(WhereClause(predicate)).Append(";").ToString();

        public string CreateSql()
            => CREATE_SQL;

        public string ReadSql()
            => READ_PREFIX.Append(";").ToString();

        public string ReadById()
            => READ_PREFIX.Append("where Id = @Id").ToString();

        public string ReadSql(Expression<Func<T, bool>> predicate)
            => READ_PREFIX.Append(WhereClause(predicate)).Append(";").ToString();

        public string ReadSql<TParam>(Expression<Func<T, TParam, bool>> predicate)
            => READ_PREFIX.Append(WhereClause(predicate)).Append(";").ToString();
        
        public string ReadSqlById()
            => READ_WHERE_ID;

        public string UpdateSql<TParam>()
        {
            var paramNames = new HashSet<string>(typeof(TParam).GetDataProperties().Select(p => p.PropertyName));
            return UPDATE_PREFIX.Append(string.Join(", ", NON_ID_COLUMNS.Where(c => paramNames.Contains(c.ColumnName)).Select(c => "[" + c.ColumnName + "] = @" + c.ColumnName))).ToString();
        }

        public string WhereClause<TParam>(Expression<Func<T, TParam, bool>> predicate)
            => "WHERE".Append(predicate.ToSql()).ToString();        

        public string WhereClause(Expression<Func<T, bool>> predicate)
            => "WHERE".Append(predicate.ToSql()).ToString();             
    }
}

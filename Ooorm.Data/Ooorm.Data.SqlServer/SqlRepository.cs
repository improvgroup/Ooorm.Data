using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    /// <summary>
    /// Generic Repository for Sql Server connections
    /// </summary>
    public class SqlRepository<T> : ICrudRepository<T> where T : IDbItem
    {
        protected readonly SqlConnection ConnectionSource;
        private readonly SqlDao dao;
        private readonly SqlServerQueryProvider<T> queries;

        public SqlRepository(SqlConnection connection, Func<IDatabase> db)
            => (ConnectionSource, dao, queries) = (connection, new SqlDao(db), new SqlServerQueryProvider<T>(db));

        public async Task<int> Write(params T[] values)
            => await ConnectionSource.FromConnectionAsync(async c => {
                var set = new HashSet<int>();
                foreach (var value in values)
                {
                    value.ID = await dao.ExecuteScalarAsync(c, queries.WriteSql(), value);
                    set.Add(value.ID);
                }
                return set.Count;
            });

        public async Task<IEnumerable<T>> Read()
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(), null)).ToList());

        public async Task<T> Read(int id)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSqlById(), new { Id = id })).Single());

        public async Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(predicate), null)).ToList());

        public async Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, object param)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(predicate), (predicate.Parameters[1].Name, param))).ToList());

        public async Task<int> Update(params T[] values)
            => await ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>(values.Length);
                foreach (var value in values)
                    list.Add(dao.ExecuteAsync(c, queries.UpdateSql<T>(), value));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });

        public async Task<int> Delete(params int[] ids)
            => await ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>(ids.Length);
                foreach (var id in ids)
                    list.Add(dao.ExecuteAsync(c, queries.WriteSql(), new { Id = id }));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });

        public async Task<int> Delete(Expression<Func<T, bool>> predicate)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.DeleteSql(predicate), null)));

        public async Task<int> CreateTable()
             => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.CreateTableSql(), null)));

        public async Task<int> DropTable()
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.DropTableSql(), null)));
    }
}

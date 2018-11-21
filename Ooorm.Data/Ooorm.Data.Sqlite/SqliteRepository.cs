using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    /// <summary>
    /// Generic Repository for Sql Server connections
    /// </summary>
    public class SqliteRepository<T> : ICrudRepository<T> where T : IDbItem
    {
        protected readonly SqliteConnection ConnectionSource;
        private readonly SqliteDao dao;
        private readonly SqliteQueryProvider<T> queries;

        public SqliteRepository(SqliteConnection connection, Func<IDatabase> db)
            => (ConnectionSource, dao, queries) = (connection, new SqliteDao(db), new SqliteQueryProvider<T>(db));

        public async Task<int> Write(params T[] values)
            => await ConnectionSource.FromConnectionAsync(async c => {
                var set = new HashSet<int>();
                foreach (var value in values)
                {
                    value.ID = (int)(await dao.ExecuteScalarAsync(c, queries.WriteSql(), value));
                    set.Add(value.ID);
                }
                return set.Count;
            });

        public async Task<IEnumerable<T>> Read()
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(), null)).ToList());

        public async Task<IEnumerable<object>> ReadUntyped()
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(), null)).Select(i => (object)i).ToList());


        public async Task<T> Read(int id)
            => await ConnectionSource.FromConnectionAsync(async c =>
            {
                var results = await dao.ReadAsync<T>(c, queries.ReadSqlById(), new { Id = id });
                return results.Single();
            });

        public async Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(predicate), null)).ToList());

        public async Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(predicate, param), (predicate.Parameters[1].Name, param))).ToList());



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

        public async Task<int> Delete(params T[] values)
            => await DeleteById(values.Select(v => v.ID));


        private async Task<int> DeleteById(IEnumerable<int> ids)
            => await ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>();
                foreach (var id in ids)
                    list.Add(dao.ExecuteAsync(c, queries.DeleteSqlById(), new { Id = id }));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });

        public async Task<int> Delete(Expression<Func<T, bool>> predicate)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.DeleteSql(predicate), null)));

        public async Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.DeleteSql(predicate, param), param)));

        public async Task<int> CreateTable()
             => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.CreateTableSql(), null)));

        public async Task<int> DropTable()
            => await ConnectionSource.FromConnectionAsync(async c => (await dao.ExecuteAsync(c, queries.DropTableSql(), null)));
    }
}

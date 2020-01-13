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
    public class SqliteRepository<T, TId> : ICrudRepository<T, TId> 
        where T : DbItem<T, TId> 
        where TId : struct, IEquatable<TId>
    {
        protected readonly SqliteConnection ConnectionSource;
        private readonly SqliteDao dao;
        private readonly SqliteQueryProvider<T, TId> queries;

        public SqliteRepository(SqliteConnection connection, Func<IDatabase> db)
            => (ConnectionSource, dao, queries) = (connection, new SqliteDao(db), new SqliteQueryProvider<T, TId>(db));

        public Task<SortedList<TId, T>> Write(params T[] values) =>
            ConnectionSource.FromConnectionAsync(async c => {
                var results = new SortedList<TId, T>();

                foreach (var value in values)
                {
                    var result = (await dao.ReadAsync<T, TId>(c, queries.WriteSql(), value)).Single();
                    value.ID = result.ID;
                    results.Add(result.ID, result);
                }                
                
                return results;
            });

        public Task<List<T>> Read() =>
            ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T, TId>(c, queries.ReadSql(), null)).ToList());

        public Task<List<object>> ReadUntyped() =>
            ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T, TId>(c, queries.ReadSql(), null)).Select(i => (object)i).ToList());


        public Task<T> Read(TId id) =>
            ConnectionSource.FromConnectionAsync(async c =>
            {
                var results = await dao.ReadAsync<T, TId>(c, queries.ReadSqlById(), new { Id = id });
                return results.Single();
            });

        public Task<List<T>> Read(Expression<Func<T, bool>> predicate) =>
            ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T, TId>(c, queries.ReadSql(predicate), null)).ToList());

        public Task<List<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) =>
            ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T, TId>(c, queries.ReadSql(predicate, param), (predicate.Parameters[1].Name, param))).ToList());

        public Task<SortedList<TId, T>> Update(params T[] values) =>
            ConnectionSource.FromConnectionAsync(async c => 
            {
                var results = new SortedList<TId, T>();
                foreach (var value in values)
                {
                    var result = (await dao.ReadAsync<T, TId>(c, queries.UpdateSql<T>(), value)).Single();
                    value.ID = result.ID;
                    results.Add(result.ID, result);
                }                
                return results;
            });

        public Task<int> Delete(params T[] values) => DeleteById(values.Select(v => v.ID));

        private Task<int> DeleteById(IEnumerable<TId> ids) =>
            ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>();
                foreach (var id in ids)
                    list.Add(dao.ExecuteAsync(c, queries.DeleteSqlById(), new { Id = id }));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });

        public Task<int> Delete(Expression<Func<T, bool>> predicate) =>
            ConnectionSource.FromConnectionAsync(c => 
                dao.ExecuteAsync(c, queries.DeleteSql(predicate), null));

        public Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) =>
            ConnectionSource.FromConnectionAsync(c => 
                dao.ExecuteAsync(c, queries.DeleteSql(predicate, param), param));

        public Task<int> CreateTable() =>
            ConnectionSource.FromConnectionAsync(c => 
                dao.ExecuteAsync(c, queries.CreateTableSql(), null));

        public Task<int> DropTable() =>
            ConnectionSource.FromConnectionAsync(c => 
                dao.ExecuteAsync(c, queries.DropTableSql(), null));
    }
}

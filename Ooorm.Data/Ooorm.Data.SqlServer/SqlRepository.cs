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
        protected readonly SqlServerConnectionSource ConnectionSource;
        protected readonly SqlDao dao = new SqlDao();
        private readonly SqlServerQueryProvider<T> queries = new SqlServerQueryProvider<T>();

        public SqlRepository(SqlServerConnectionSource connection) => ConnectionSource = connection;        

        public async Task<int> Create(params T[] values)
            => await await ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>(values.Length);
                foreach (var value in values)
                    list.Add(dao.ExecuteAsync(c, queries.CreateSql(), value));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });        

        public async Task<IEnumerable<T>> Read()
            => await await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(), null)).ToList());        

        public async Task<T> Read(int id)        
            => await await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSqlById(), new { Id = id })).Single());        

        public async Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate)
            => await await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(predicate), null)).ToList());
        
        public async Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
            => await await ConnectionSource.FromConnectionAsync(async c => (await dao.ReadAsync<T>(c, queries.ReadSql(predicate), param)).ToList());        

        public async Task<int> Update(params T[] values)        
            => await await ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>(values.Length);
                foreach (var value in values)
                    list.Add(dao.ExecuteAsync(c, queries.UpdateSql<T>(), value));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });        

        public async Task<int> Delete(params int[] ids)
            => await await ConnectionSource.FromConnectionAsync(async c => {
                var list = new List<Task<int>>(ids.Length);
                foreach (var id in ids)
                    list.Add(dao.ExecuteAsync(c, queries.CreateSql(), new { ID = id }));
                int sum = 0;
                foreach (var task in list)
                    sum += await task;
                return sum;
            });        
    }
}

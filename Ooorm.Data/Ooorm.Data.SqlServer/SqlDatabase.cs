using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.SqlServer
{
    public class SqlDatabase : IDatabaseManagementSystem
    {
        private readonly SqlServerQueryProvider queries = new SqlServerQueryProvider();

        private readonly SqlDao dao;

        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private ICrudRepository<T, TId> Repos<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => (ICrudRepository<T, TId>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new SqlRepository<T, TId>(source, () => this)));       

        private readonly SqlConnection source;

        public SqlDatabase(SqlConnection source) => (this.source, dao) = (source, new SqlDao(() => this));

        public Task<SortedList<TId, T>> Write<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Write(values);

        public Task<int> Delete<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(values);

        public Task<int> Delete<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(predicate);

        public Task<int> Delete<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(predicate, param);

        public Task<List<T>> Read<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read();

        public Task<T> Read<T, TId>(TId id) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(id);

        public Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(predicate);

        public Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(predicate, param);       

        public Task<SortedList<TId, T>> Update<T, TId>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Update(values);

        public Task<T> Dereference<T, TId>(DbVal<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(value);

        public async Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => value.IsNull ? (false, default) : (true, await Repos<T, TId>().Read(((TId?)value).Value));

        public Task CreateTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().CreateTable();

        public Task DropTable<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => Repos<T, TId>().DropTable();

        /// <summary>
        /// Drops a database if it exists
        /// </summary>
        public Task DropDatabase(string name) => source.WithConnectionAsync(c => dao.ExecuteAsync(c, queries.DropDatabaseSql(name), null));

        public Task CreateDatabase(string name) => source.WithConnectionAsync(c => dao.ExecuteAsync(c, queries.DatabaseSql(name), null));

        public Task<List<T>> Read<T, TId>(Expression<Func<T>> constructor)
            where T : DbItem<T, TId>
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(constructor);

        public Task<int> Delete<T, TId>(Expression<Func<T>> constructor)
            where T : DbItem<T, TId>
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(constructor);
    }
}

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

        private ICrudRepository<T> Repos<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => (ICrudRepository<T>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new SqlRepository<T>(source, () => this)));

        private ICrudRepository Repos(Type type)
            => (ICrudRepository)(repositories.ContainsKey(type) ? repositories[type] : (repositories[type] = Activator.CreateInstance(typeof(SqlRepository<>).MakeGenericType(type), source, (Func<IDatabase>)(() => this))));

        private readonly SqlConnection source;

        public SqlDatabase(SqlConnection source) => (this.source, dao) = (source, new SqlDao(() => this));

        public async Task<int> Write<T>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Write(values);

        public async Task<int> Delete<T>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Delete(values);

        public async Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Delete(predicate);

        public async Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Delete(predicate, param);

        public async Task<List<T>> Read<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read();

        public async Task<T> Read<T>(int id) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(id);

        public async Task<List<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(predicate);

        public async Task<List<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(predicate, param);

        public async Task<List<object>> Read(Type type)
            => await Repos(type).ReadUntyped();

        public async Task<int> Update<T>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Update(values);

        public async Task<T> Dereference<T>(DbVal<T> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(value);

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => value.IsNull ? (false, default) : (true, await Repos<T>().Read(((int?)value).Value));

        public async Task CreateTable<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().CreateTable();

        public async Task DropTable<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().DropTable();

        public async Task CreateTables(params Type[] tables)
        {
            foreach (var type in tables)
                await Repos(type).CreateTable();
        }

        public async Task DropTables(params Type[] tables)
        {
            foreach (var type in tables)
                await Repos(type).DropTable();
        }

        /// <summary>
        /// Drops a database if it exists
        /// </summary>
        public async Task DropDatabase(string name)
            => await source.WithConnectionAsync(async c
                => await dao.ExecuteAsync(c, queries.DropDatabaseSql(name), null));

        /// <summary>
        /// Creates a database if it doesn't exist
        /// </summary>
        public async Task CreateDatabase(string name, params Type[] tables)
        {
            await source.WithConnectionAsync(async c => await dao.ExecuteAsync(c, queries.DatabaseSql(name), null));
            if (tables.Length > 0)
                await source.WithConnectionAsync(async c =>
                {
                    c.ChangeDatabase(name);
                    foreach (var type in tables)
                        await Repos(type).CreateTable();
                });
        }


    }
}

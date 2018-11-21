using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Sqlite
{
    public class SqliteDatabase : IDatabase
    {        
        private readonly SqliteDao dao;

        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private ICrudRepository<T> Repos<T>() where T : IDbItem
            => (ICrudRepository<T>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new SqliteRepository<T>(source, () => this)));

        private ICrudRepository Repos(Type type)
            => (ICrudRepository)(repositories.ContainsKey(type) ? repositories[type] : (repositories[type] = Activator.CreateInstance(typeof(SqliteRepository<>).MakeGenericType(type), source, (Func<IDatabase>)(() => this))));

        private readonly SqliteConnection source;

        public SqliteDatabase(SqliteConnection source) => (this.source, dao) = (source, new SqliteDao(() => this));

        public async Task<int> Write<T>(params T[] values) where T : IDbItem
            => await Repos<T>().Write(values);

        public async Task<int> Delete<T>(params T[] values) where T : IDbItem
            => await Repos<T>().Delete(values);

        public async Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
            => await Repos<T>().Delete(predicate);

        public async Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
            => await Repos<T>().Delete(predicate, param);

        public async Task<IEnumerable<T>> Read<T>() where T : IDbItem
            => await Repos<T>().Read();

        public async Task<T> Read<T>(int id) where T : IDbItem
            => await Repos<T>().Read(id);

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
            => await Repos<T>().Read(predicate);

        public async Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
            => await Repos<T>().Read(predicate, param);

        public async Task<IEnumerable<object>> Read(Type type)
            => await Repos(type).ReadUntyped();

        public async Task<int> Update<T>(params T[] values) where T : IDbItem
            => await Repos<T>().Update(values);

        public async Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
            => await Repos<T>().Read(value);

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
            => value.IsNull ? (false, default) : (true, await Repos<T>().Read(((int?)value).Value));

        public async Task CreateTable<T>() where T : IDbItem
            => await Repos<T>().CreateTable();

        public async Task DropTable<T>() where T : IDbItem
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
    }
}

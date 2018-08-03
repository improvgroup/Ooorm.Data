using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Volatile
{
    public class VolatileDatabase : IDatabase
    {
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private ICrudRepository<T> Repos<T>() where T : IDbItem
            => (ICrudRepository<T>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new VolatileRepository<T>(() => this)));

        private ICrudRepository Repos(Type type)
            => (ICrudRepository)(repositories.ContainsKey(type) ? repositories[type] : (repositories[type] = Activator.CreateInstance(typeof(VolatileRepository<>).MakeGenericType(type), (Func<IDatabase>)(() => this))));

        public async Task<int> Write<T>(params T[] values) where T : IDbItem
            => await Repos<T>().Write(values);

        public async Task<IEnumerable<T>> Read<T>() where T : IDbItem
            => await Repos<T>().Read();

        public async Task<T> Read<T>(int id) where T : IDbItem
            => await Repos<T>().Read(id);

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
            => await Repos<T>().Read(predicate);

        public async Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
            => await Repos<T>().Read(predicate, param);

        public async Task<int> Update<T>(params T[] values) where T : IDbItem
            => await Repos<T>().Update(values);

        public async Task<int> Delete<T>(params int[] ids) where T : IDbItem
            => await Repos<T>().Delete(ids);

        public async Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
            => await Repos<T>().Delete(predicate);

        public async Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
            => await Repos<T>().Delete(predicate, param);

        public async Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
            => await Repos<T>().Read(value.ToId());

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
            => (!value.IsNull, value.IsNull ? default : await Repos<T>().Read(value.ToId().Value));

        public async Task DropDatabase(string name) { }

        public async Task CreateDatabase(string name, params Type[] tables) { }

        public async Task CreateTable<T>() where T : IDbItem { }

        public async Task CreateTables(params Type[] tables) { }

        public VolatileDatabase()
        {

        }
    }
}

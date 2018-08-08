using Ooorm.Data.Volatile;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.SignalrClient
{
    public class SignalrDatabase : IDatabase
    {
        private readonly string url;

        internal readonly VolatileDatabase LocalDb = new VolatileDatabase();

        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private async Task LoadRepository<T>() where T : IDbItem =>
            repositories[typeof(T)] = new SignalrRepository<T>(url, () => this);

        private async Task LoadRepository(Type t) =>
            repositories[t] = Activator.CreateInstance(typeof(SignalrRepository<>).MakeGenericType(t), url, ((Func<SignalrDatabase>)(() => this)));

        private SignalrRepository<T> Repos<T>() where T : IDbItem
            => (SignalrRepository<T>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : throw new InvalidOperationException($"Repository for type {typeof(T).Name} has not been loaded"));

        private ICrudRepository Repos(Type type)
            => (ICrudRepository)(repositories.ContainsKey(type) ? repositories[type] : throw new InvalidOperationException($"Repository for type {type.Name} has not been loaded"));

        public Task CreateDatabase(string name, params Type[] tables) => throw new NotSupportedException();
        public Task DropDatabase(string name) => throw new NotSupportedException();

        public async Task LoadTable<T>() where T : IDbItem
        {
            await LoadRepository<T>();
            await Repos<T>().WaitForLoad();
        }

        public async Task CreateTable<T>() where T : IDbItem => await LoadRepository<T>();

        public async Task CreateTables(params Type[] tables)
        {
            foreach (var type in tables)
                await LoadRepository(type);
        }

        public SignalrDatabase(string url) => this.url = url + "/ooorm";

        public async Task<int> Delete<T>(params int[] ids) where T : IDbItem =>
            await Repos<T>().Delete(ids);

        public async Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem =>
            await Repos<T>().Delete(predicate);

        public async Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem =>
            await Repos<T>().Delete(predicate, param);

        public async Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem =>
            await Repos<T>().Read(value.ToId());

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem =>
            value.IsNull ? (false, default(T)) : (true, await Repos<T>().Read(value.ToId().Value));

        public async Task<IEnumerable<T>> Read<T>() where T : IDbItem =>
            await Repos<T>().Read();

        public async Task<T> Read<T>(int id) where T : IDbItem =>
            await Repos<T>().Read(id);

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem =>
            await Repos<T>().Read(predicate);

        public async Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem =>
            await Repos<T>().Read(predicate, param);

        public async Task<int> Update<T>(params T[] values) where T : IDbItem =>
            await Repos<T>().Update(values);

        public async Task<int> Write<T>(params T[] values) where T : IDbItem =>
            await Repos<T>().Write(values);
    }
}

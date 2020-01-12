using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Volatile
{
    public class VolatileDatabase : IDatabase
    {
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private ICrudRepository<T> Repos<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => (ICrudRepository<T>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new VolatileRepository<T>(() => this)));

        private ICrudRepository Repos(Type type)
            => (ICrudRepository)(repositories.ContainsKey(type) ? repositories[type] : (repositories[type] = Activator.CreateInstance(typeof(VolatileRepository<>).MakeGenericType(type), (Func<IDatabase>)(() => this))));

        public async Task<int> Write<T>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Write(values);

        public async Task<List<T>> Read<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read();

        public async Task<T> Read<T>(int id) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(id);

        public async Task<List<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(predicate);

        public async Task<List<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(predicate, param);

        public async Task<int> Update<T>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Update(values);

        public async Task<int> Delete<T>(params T[] values) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Delete(values);

        public async Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Delete(predicate);

        public async Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Delete(predicate, param);

        public async Task<T> Dereference<T>(DbVal<T> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await Repos<T>().Read(value.ToId());

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => (!value.IsNull, value.IsNull ? default : await Repos<T>().Read(value.ToId().Value));

        public async Task<List<object>> Read(Type type)
            => await Repos(type).ReadUntyped();

        public Task CreateTable<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            Repos<T>();
            return Task.CompletedTask;
        }

        public Task CreateTables(params Type[] tables)
        {
            foreach (var type in tables)
                Repos(type);
            return Task.CompletedTask;
        }

        public Task DropTable<T>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
        {
            if (repositories.ContainsKey(typeof(T)))
                repositories.Remove(typeof(T));
            return Task.CompletedTask;
        }

        public Task DropTables(params Type[] tables)
        {
            foreach (var type in tables)
                if (repositories.ContainsKey(type))
                    repositories.Remove(type);
            return Task.CompletedTask;
        }

        public VolatileDatabase()
        {

        }
    }
}

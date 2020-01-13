using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.Volatile
{
    public class VolatileDatabase : IDatabase
    {
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        private ICrudRepository<T, TId> Repos<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => (ICrudRepository<T, TId>)(repositories.ContainsKey(typeof(T)) ? repositories[typeof(T)] : (repositories[typeof(T)] = new VolatileRepository<T, TId>(() => this)));

        public Task<SortedList<TId, T>> Write<T, TId>(params T[] values) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Write(values);

        public Task<List<T>> Read<T, TId>() where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read();

        public Task<T> Read<T, TId>(TId id) where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(id);

        public Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(predicate);

        public Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param)
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(predicate, param);

        public Task<SortedList<TId, T>> Update<T, TId>(params T[] values) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Update(values);

        public Task<int> Delete<T, TId>(params T[] values) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(values);

        public Task<int> Delete<T, TId>(Expression<Func<T, bool>> predicate) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(predicate);

        public Task<int> Delete<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Delete(predicate, param);

        public Task<T> Dereference<T, TId>(DbVal<T, TId> value) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => Repos<T, TId>().Read(value.ToId());

        public async Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => (!value.IsNull, value.IsNull ? default : await Repos<T, TId>().Read(value.ToId().Value));

        public Task CreateTable<T, TId>() 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
        {
            Repos<T, TId>();
            return Task.CompletedTask;
        }

        public Task DropTable<T, TId>() 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
        {
            if (repositories.ContainsKey(typeof(T)))
                repositories.Remove(typeof(T));
            return Task.CompletedTask;
        }

        public VolatileDatabase() { }
    }
}

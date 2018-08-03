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

        public Task<int> Write<T>(params T[] values) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<T>() where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<T> Read<T>(int id) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Update<T>(params T[] values) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete<T>(params int[] ids) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task DropDatabase(string name)
        {
            throw new NotImplementedException();
        }

        public Task CreateDatabase(string name, params Type[] tables)
        {
            throw new NotImplementedException();
        }

        public Task CreateTable<T>() where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task CreateTables(params Type[] tables)
        {
            throw new NotImplementedException();
        }

        public VolatileDatabase()
        {

        }
    }
}

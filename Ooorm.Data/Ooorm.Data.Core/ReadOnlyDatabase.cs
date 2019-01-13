using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ooorm.Data.Core
{
    public class ReadOnlyDatabase : IReadable
    {
        private readonly IDatabase source;

        public ReadOnlyDatabase(IDatabase source) => this.source = source;

        public async Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
            => await source.Dereference(value);

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
            => await source.Dereference(value);

        public IAsyncEnumerable<T> Read<T>() where T : IDbItem
            => source.Read<T>();

        public async Task<T> Read<T>(int id) where T : IDbItem
            => await source.Read<T>(id);

        public IAsyncEnumerable<T> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
            => source.Read(predicate);

        public IAsyncEnumerable<T> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
            => source.Read(predicate, param);

        public IAsyncEnumerable<object> Read(Type type)
            => source.Read(type);
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public class ReadOnlyDatabase : IReadable
    {
        private readonly IDatabase source;

        public ReadOnlyDatabase(IDatabase source) => this.source = source;

        public async Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
            => await source.Dereference(value);

        public async Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
            => await source.Dereference(value);

        public async Task<IEnumerable<T>> Read<T>() where T : IDbItem
            => await source.Read<T>();

        public async Task<T> Read<T>(int id) where T : IDbItem
            => await source.Read<T>(id);

        public async Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
            => await source.Read(predicate);

        public async Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
            => await source.Read(predicate, param);

        public async Task<IEnumerable<object>> Read(Type type)
            => await source.Read(type);
    }
}

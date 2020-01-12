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

        public async Task<T> Dereference<T, TId>(DbVal<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await source.Dereference(value);

        public async Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await source.Dereference(value);

        public async Task<List<T>> Read<T, TId>() where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await source.Read<T, TId>();

        public async Task<T> Read<T, TId>(TId id) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await source.Read<T, TId>(id);

        public async Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await source.Read<T, TId>(predicate);

        public async Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
            => await source.Read<T, TParam, TId>(predicate, param);

        public async Task<List<object>> Read(Type type)
            => await source.Read(type);
    }
}

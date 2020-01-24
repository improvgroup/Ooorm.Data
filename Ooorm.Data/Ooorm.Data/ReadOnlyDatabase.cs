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

        public Task<T> Dereference<T, TId>(DbVal<T, TId> value) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => source.Dereference(value);

        public Task<(bool exists, T value)> Dereference<T, TId>(DbRef<T, TId> value) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => source.Dereference(value);

        public Task<List<T>> Read<T, TId>() 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => source.Read<T, TId>();

        public Task<T> Read<T, TId>(TId id) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => source.Read<T, TId>(id);

        public Task<List<T>> Read<T, TId>(Expression<Func<T, bool>> predicate) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => source.Read<T, TId>(predicate);

        public Task<List<T>> Read<T, TParam, TId>(Expression<Func<T, TParam, bool>> predicate, TParam param) 
            where T : DbItem<T, TId> 
            where TId : struct, IEquatable<TId>
            => source.Read<T, TParam, TId>(predicate, param);

        public Task<List<T>> Read<T, TId>(Expression<Func<T>> constructor)
            where T : DbItem<T, TId>
            where TId : struct, IEquatable<TId>
            => source.Read<T, TId>(constructor);
    }
}

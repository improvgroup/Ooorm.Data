using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public struct DbRef<T, TId> : IdConvertable<TId?> where T : IDbItem<TId> where TId : struct, IEquatable<TId>
    {
        internal readonly Func<IDatabase> getDb;

        internal readonly TId? value;

        public bool IsNull => !value.HasValue;

        public bool HasValue => value.HasValue;

        public DbRef(TId? v, Func<IDatabase> db) => (value, getDb) = (v, db);

        public static implicit operator TId? (DbRef<T, TId> v) => v.value;

        public static implicit operator DbRef<T, TId> (DbVal<T, TId> v) => new DbRef<T, TId>(v.value, v.getDb);

        public TId? ToId() => value;

        public async Task<T> Get() => value.HasValue ? await getDb()?.Read<T, TId>(value.Value) : default;

        public async Task<object> GetObject() => await Get();

        public static bool operator ==(DbRef<T, TId> a, DbRef<T, TId> b) => a.HasValue && b.HasValue && a.value.Equals(b.value);
        public static bool operator !=(DbRef<T, TId> a, DbRef<T, TId> b) => !a.HasValue || !b.HasValue || !a.value.Equals(b.value);
        public static bool operator ==(DbRef<T, TId> a, DbVal<T, TId> b) => a.HasValue && a.value.Equals(b.value);
        public static bool operator !=(DbRef<T, TId> a, DbVal<T, TId> b) => !a.HasValue || !a.value.Equals(b.value);

        public override bool Equals(object obj)
        {
            if (obj is DbRef<T, TId> r)
                return this == r;
            else if (obj is DbVal<T, TId> v)
                return this == v;
            else
                return base.Equals(obj);
        }

        public override int GetHashCode() => -1584136870 + EqualityComparer<TId?>.Default.GetHashCode(value);


    }
}

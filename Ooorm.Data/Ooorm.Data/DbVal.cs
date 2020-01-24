using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public struct DbVal<T, TId> : IdConvertable<TId> where T : DbItem<T, TId> where TId : struct, IEquatable<TId>
    {
        internal readonly Func<IDatabase> getDb;

        internal readonly TId value;

        public bool HasValue => true;

        public DbVal(TId v, Func<IDatabase> db) => (value, getDb) = (v, db);

        public static implicit operator TId(DbVal<T, TId> v) => v.value;

        public static implicit operator DbVal<T, TId>(DbRef<T, TId> v) => v.value.HasValue ? new DbVal<T, TId>(v.value.Value, v.getDb) : throw new NullReferenceException("Cannot cast null id to non null id");

        public TId ToId() => value;

        public async Task<T> Get() => await getDb()?.Read<T, TId>(value);

        public async Task<object> GetObject() => await Get();

        public static bool operator ==(DbVal<T, TId> a, DbVal<T, TId> b) => a.value.Equals(b.value);
        public static bool operator !=(DbVal<T, TId> a, DbVal<T, TId> b) => !a.value.Equals(b.value);
        public static bool operator ==(DbVal<T, TId> a, DbRef<T, TId> b) => b.HasValue && a.value.Equals(b.value);
        public static bool operator !=(DbVal<T, TId> a, DbRef<T, TId> b) => !b.HasValue || !a.value.Equals(b.value);

        public override bool Equals(object obj)
        {
            if (obj is DbRef<T, TId> r)
                return this == r;
            else if (obj is DbVal<T, TId> v)
                return this == v;
            else
                return base.Equals(obj);
        }

        public override int GetHashCode() => -1584136870 + value.GetHashCode();
    }
}

using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public struct DbRef<T> : IdConvertable<int?> where T : IDbItem
    {
        internal readonly Func<IDatabase> getDb;

        internal readonly int? value;

        public bool IsNull => !value.HasValue;

        public DbRef(int? v, Func<IDatabase> db) => (value, getDb) = (v, db);

        public static implicit operator int? (DbRef<T> v) => v.value;

        public static implicit operator DbRef<T> (DbVal<T> v) => new DbRef<T>(v.value, v.getDb);

        public int? ToId() => value;

        public async Task<T> Get() => value.HasValue ? await getDb()?.Read<T>(value.Value) : default;

        public static bool operator ==(DbRef<T> a, DbRef<T> b) => a.value == b.value;
        public static bool operator !=(DbRef<T> a, DbRef<T> b) => a.value != b.value;
        public static bool operator ==(DbRef<T> a, DbVal<T> b) => a.value == b.value;
        public static bool operator !=(DbRef<T> a, DbVal<T> b) => a.value != b.value;

        public override bool Equals(object obj)
        {
            if (obj is DbRef<T> r)
                return this == r;
            else if (obj is DbVal<T> v)
                return this == v;
            else
                return base.Equals(obj);
        }
    }
}

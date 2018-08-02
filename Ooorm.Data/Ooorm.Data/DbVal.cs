using System;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public struct DbVal<T> : IdConvertable<int> where T : IDbItem
    {
        internal readonly Func<IDatabase> getDb;

        internal readonly int value;

        public DbVal(int v, Func<IDatabase> db) => (value, getDb) = (v, db);

        public static implicit operator int(DbVal<T> v) => v.value;

        public static implicit operator DbVal<T>(DbRef<T> v) => v.value.HasValue ? new DbRef<T>(v.value.Value, v.getDb) : throw new NullReferenceException("Cannot cast null id to non null id");

        public int ToId() => value;

        public async Task<T> Get() => await getDb()?.Read<T>(value);

        public static bool operator ==(DbVal<T> a, DbVal<T> b) => a.value == b.value;
        public static bool operator !=(DbVal<T> a, DbVal<T> b) => a.value != b.value;
        public static bool operator ==(DbVal<T> a, DbRef<T> b) => a.value == b.value;
        public static bool operator !=(DbVal<T> a, DbRef<T> b) => a.value != b.value;

        public override bool Equals(object obj)
        {
            if (obj is DbRef<T> r)
                return this == r;
            else if (obj is DbVal<T> v)
                return this == v;
            else
                return base.Equals(obj);
        }

        public override int GetHashCode() => -1584136870 + value.GetHashCode();
    }
}

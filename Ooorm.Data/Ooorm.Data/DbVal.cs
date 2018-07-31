namespace Ooorm.Data
{
    public struct DbVal<T> : IdConvertable<int> where T : IDbItem
    {
        private readonly int value;

        public DbVal(int v) => value = v;

        public static implicit operator DbVal<T>(int v) => new DbVal<T>(v);

        public static implicit operator DbVal<T>(T v) => new DbVal<T>(v.ID);

        public static implicit operator int(DbVal<T> v) => v.value;

        public int ToId() => value;
    }
}

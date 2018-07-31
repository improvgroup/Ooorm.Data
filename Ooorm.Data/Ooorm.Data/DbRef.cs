namespace Ooorm.Data
{
    public struct DbRef<T> : IdConvertable<int?> where T : IDbItem
    {
        private readonly int? value;

        public bool IsNull => !value.HasValue;

        public DbRef(int? v) => value = v;

        public static implicit operator DbRef<T>(int? v) => new DbRef<T>(v);

        public static implicit operator DbRef<T>(T v) => new DbRef<T>(v.ID);

        public static implicit operator int?(DbRef<T> v) => v.value;

        public int? ToId() => value;
    }
}

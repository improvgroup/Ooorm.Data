namespace Ooorm.Data
{
    public interface IDbItem
    {
        int ID { get; set; }
    }

    public interface IdConvertable<TId>
    {
        TId ToId();
    }

    public struct DbVal<T> : IdConvertable<int> where T : IDbItem
    {
        private readonly int value;

        public DbVal(int v) => value = v;

        public static implicit operator DbVal<T>(int v) => new DbVal<T>(v);

        public static implicit operator DbVal<T>(T v) => new DbVal<T>(v.ID);

        public static implicit operator int(DbVal<T> v) => v.value;

        public int ToId() => value;
    }

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

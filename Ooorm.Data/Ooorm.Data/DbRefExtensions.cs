namespace Ooorm.Data
{
    public static class DbRefExtensions
    {
        public static DbVal<T> In<T>(this T item, IDatabase database) where T : IDbItem
            => new DbVal<T>(item.ID, () => database);
    }
}

using System.Collections.Generic;

namespace Ooorm.Data
{
    public static class DbRefExtensions
    {
        public static DbVal<T> In<T>(this T item, IDatabase database) where T : IDbItem
            => item.ID != default ? new DbVal<T>(item.ID, () => database) : throw new KeyNotFoundException("Cannot add reference reference to an item without a DB");
    }
}

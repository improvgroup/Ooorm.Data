using System.Collections.Generic;

namespace Ooorm.Data
{
    public static class DbRefExtensions
    {
        public static DbRef<T> In<T>(this T item, IDatabase database) where T : IDbItem
        {
            if (item == null)
                return new DbRef<T>(null, () => database);
            return item.ID != default ? new DbRef<T>(item.ID, () => database) : throw new KeyNotFoundException("Cannot add reference reference to an item without a DB");
        }
    }
}

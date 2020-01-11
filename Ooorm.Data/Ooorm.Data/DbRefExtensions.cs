using System;
using System.Collections.Generic;

namespace Ooorm.Data
{
    public static class DbRefExtensions
    {
        public static DbRef<T, TId> In<T, TId>(this T item, IDatabase database) where T : IDbItem<TId> where TId : struct, IEquatable<TId>
        {
            if (item == null)
                return new DbRef<T, TId>(null, () => database);
            return item.IsNew ? new DbRef<T, TId>(item.ID, () => database) : throw new KeyNotFoundException("Cannot add reference reference to an item without a DB");
        }
    }
}

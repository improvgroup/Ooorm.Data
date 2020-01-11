using System;

namespace Ooorm.Data
{
    public interface IObservableCrud<T, TId> : ICrudRepository<T, TId> where T : IDbItem<TId> where TId : struct, IEquatable<TId>
    {
        event Action<T> OnCreated;
        event Action<T,T> OnUpdated;
        event Action<int> OnDeleted;
    }
}

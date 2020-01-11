using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IObservableDbItem<T, TId> : INotifyPropertyChanged where T : IDbItem<T, TId> where TId : struct, IEquatable<TId>
    {
        IObservableCrud<T, TId> _Repo { get; }
        T _Data { get; }
        void Set<TProp>(string property, T value);
        TProp Get<TProp>(string property);
        Task Commit();
    }
}

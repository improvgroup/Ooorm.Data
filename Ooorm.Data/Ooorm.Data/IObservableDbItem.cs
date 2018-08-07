using System.ComponentModel;
using System.Threading.Tasks;

namespace Ooorm.Data
{
    public interface IObservableDbItem<T> : INotifyPropertyChanged where T : IDbItem
    {
        IObservableCrud<T> _Repo { get; }
        T _Data { get; }
        void Set<TProp>(string property, T value);
        TProp Get<TProp>(string property);
        Task Commit();
    }
}

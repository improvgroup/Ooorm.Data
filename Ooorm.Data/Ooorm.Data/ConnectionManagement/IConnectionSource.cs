using System;
using System.Data;
using System.Threading.Tasks;

namespace Ooorm.Data.ConnectionManagement
{
    public interface IConnectionSource<TDbConnection> : IDisposable where TDbConnection : IDbConnection
    {
        void WithConnection(Action<TDbConnection> action);
        Task WithConnectionAsync(Action<TDbConnection> action);

        T FromConnection<T>(Func<TDbConnection, T> action);
        Task<T> FromConnectionAsync<T>(Func<TDbConnection, Task<T>> action);

        int OpenConnections { get; }
        bool CanConnect { get; }
    }
}

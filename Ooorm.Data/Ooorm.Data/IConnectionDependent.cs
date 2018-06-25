using System;
using System.Data;

namespace Ooorm.Data
{
    public interface IConnectionDependent<TDbConnection> where TDbConnection : IDbConnection
    {
        Func<TDbConnection> ConnectionSource { get; }
    }
}

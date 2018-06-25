using System;
using System.Data;

namespace Ooorm.Data.Schemas
{
    public abstract class DatabaseBuilder<TDbConnection> : ConnectedBuilder<DatabaseBuilder<TDbConnection>, TDbConnection> where TDbConnection : IDbConnection
    {
        protected DatabaseBuilder(Func<TDbConnection> source) : base(source) { }

        protected DatabaseBuilder(DatabaseBuilder<TDbConnection> parent, Action<IDbTransaction> action) : base(parent, action) { }
    }
}

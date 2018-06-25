using System;
using System.Data;

namespace Ooorm.Data
{
    public abstract class ConnectedBuilder<TBuilder, TDbConnection> : IConnectionDependent<TDbConnection> where TDbConnection : IDbConnection where TBuilder : ConnectedBuilder<TBuilder, TDbConnection>
    {
        protected readonly TBuilder parent;
        protected readonly Action<IDbTransaction> action;
        protected readonly Func<TDbConnection> connection;
        public Func<TDbConnection> ConnectionSource => parent?.ConnectionSource ?? connection;

        protected ConnectedBuilder(Func<TDbConnection> source)
        {
            connection = source;
            parent = null;
            action = t => { };
        }

        protected ConnectedBuilder(TBuilder parent, Action<IDbTransaction> action)
        {
            connection = null;
            this.parent = parent;
            this.action = action;
        }
    }
}

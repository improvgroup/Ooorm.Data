using Microsoft.AspNetCore.SignalR.Client;
using Ooorm.Data.Volatile;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ooorm.Data.SignalrClient
{
    public class SignalrDatabase : IDatabase
    {
        internal const string ClientRecieveItemAdded = nameof(ClientRecieveItemAdded);

        internal readonly VolatileDatabase LocalDb = new VolatileDatabase();

        private readonly HubConnection hub;

        public Task CreateDatabase(string name, params Type[] tables) => throw new NotSupportedException();
        public Task DropDatabase(string name) => throw new NotSupportedException();
        public Task CreateTable<T>() where T : IDbItem => throw new NotSupportedException();
        public Task CreateTables(params Type[] tables) => throw new NotSupportedException();

        public SignalrDatabase(string url)
        {
            hub = new HubConnectionBuilder().WithUrl(url + "/ooorm").Build();
            hub.On(ClientRecieveItemAdded, new Type[] { typeof(IDbItem) }, async (o) =>
            {

            });
        }

        private async Task recieveItemAdded(object[] items)
        {

        }

        public Task<int> Delete<T>(params int[] ids) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<T> Dereference<T>(DbVal<T> value) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<(bool exists, T value)> Dereference<T>(DbRef<T> value) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<T>() where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<T> Read<T>(int id) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<T, TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Update<T>(params T[] values) where T : IDbItem
        {
            throw new NotImplementedException();
        }

        public Task<int> Write<T>(params T[] values) where T : IDbItem
        {
            throw new NotImplementedException();
        }
    }

    public class SignalrRepository<T> : ICrudRepository<T> where T : IDbItem
    {
        private readonly string url;
        private readonly Func<SignalrDatabase> getDb;
        private readonly HubConnection hub;
        private readonly VolatileRepository<T> localRepo;

        public SignalrRepository(string url, Func<SignalrDatabase> get)
        {
            this.url = url + "/" + typeof(T).Name;
            getDb = get;
            localRepo = new VolatileRepository<T>(() => getDb().LocalDb);
            hub = new HubConnectionBuilder().WithUrl(url).Build();
        }

        public async Task<int> CreateTable() =>
            await localRepo.CreateTable();


        public async Task<int> Delete(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
        {
            throw new NotImplementedException();
        }

        public Task<int> DropTable()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read()
        {
            throw new NotImplementedException();
        }

        public Task<T> Read(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
        {
            throw new NotImplementedException();
        }

        public Task<int> Update(params T[] values)
        {
            throw new NotImplementedException();
        }

        public Task<int> Write(params T[] values)
        {
            throw new NotImplementedException();
        }
    }
}

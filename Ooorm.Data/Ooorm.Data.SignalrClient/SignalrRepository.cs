using Microsoft.AspNetCore.SignalR.Client;
using Ooorm.Data.Volatile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ooorm.Data.SignalrClient
{
    internal class SignalrRepository<T> : IObservableCrud<T> where T : IDbItem
    {
        internal const string ClientRecieveItemAdded = nameof(ClientRecieveItemAdded);
        internal const string ServerAddItem = nameof(ServerAddItem);

        internal const string ClientRecieveItemDeleted = nameof(ClientRecieveItemDeleted);
        internal const string ServerDeleteItem = nameof(ServerDeleteItem);

        internal const string ClientRecieveItemUpdated = nameof(ClientRecieveItemUpdated);
        internal const string ServerUpdateItem = nameof(ServerUpdateItem);

        internal const string ClientLoadItem = nameof(ClientLoadItem);
        internal const string ClientLoaded = nameof(ClientLoaded);

        internal const string ClientSetId = nameof(ClientSetId);

        private bool loaded = false;
        private readonly ManualResetEvent loadedTrigger = new ManualResetEvent(false);

        private readonly ManualResetEvent recieveRepoIdTrigger = new ManualResetEvent(false);
        private Guid _self;
        private readonly Guid Self;

        private readonly string url;
        private readonly Func<SignalrDatabase> getDb;
        private readonly HubConnection hub;
        private readonly VolatileRepository<T> localRepo;

        private readonly RequestCollection<T> writeRequests = new RequestCollection<T>();
        private readonly RequestCollection<T> updateRequests = new RequestCollection<T>();
        private readonly RequestCollection<int> deleteRequests = new RequestCollection<int>();

        public event Action<T> OnCreated;
        public event Action<T, T> OnUpdated;
        public event Action<int> OnDeleted;

        public SignalrRepository(string url, Func<SignalrDatabase> get)
        {
            this.url = url + "/" + typeof(T).Name;
            getDb = get;
            localRepo = new VolatileRepository<T>(() => getDb().LocalDb, manageIds: false);
            hub = new HubConnectionBuilder().WithUrl(url).Build();
            hub.On<Message<T>>(ClientRecieveItemAdded, recieveItemAdded);
            hub.On<Message<int>>(ClientRecieveItemDeleted, recieveItemDeleted);
            hub.On<Message<T>>(ClientRecieveItemUpdated, recieveItemUpdated);
            hub.On<Message<T>>(ClientLoadItem, recieveItemInitialLoad);
            hub.On<Message<Guid>>(ClientLoaded, recieveLoaded);
            hub.On<Message<Guid>>(ClientSetId, recieveRepoId);
            hub.StartAsync();

            recieveRepoIdTrigger.WaitOne();

            Self = _self;
        }

        Queue<Action> loadActionBuffer = new Queue<Action>();

        private void recieveItemAdded(Message<T> itemMessage)
        {
            if (!loaded)
            {
                loadActionBuffer.Enqueue(() =>
                {
                    localRepo.Write(itemMessage.Payload).Wait();
                    OnCreated?.Invoke(itemMessage.Payload);
                });
                return;
            }
            loadedTrigger.WaitOne();
            localRepo.Write(itemMessage.Payload).Wait();
            OnCreated?.Invoke(itemMessage.Payload);
            if (itemMessage.EndpointId == Self)
            {
                var record = writeRequests[itemMessage.MessageId];
                record.Message = itemMessage;
                record.Recieved.Set();
                writeRequests.Remove(record);
            }
        }

        private void recieveItemDeleted(Message<int> itemMessage)
        {
            if (!loaded)
            {
                loadActionBuffer.Enqueue(() =>
                {
                    localRepo.Delete(itemMessage.Payload).Wait();
                    OnDeleted?.Invoke(itemMessage.Payload);
                });
                return;
            }
            loadedTrigger.WaitOne();
            localRepo.Delete(itemMessage.Payload).Wait();
            OnDeleted?.Invoke(itemMessage.Payload);
            if (itemMessage.EndpointId == Self)
            {
                var record = deleteRequests[itemMessage.MessageId];
                record.Recieved.Set();
                deleteRequests.Remove(record);
            }
        }

        private void recieveItemUpdated(Message<T> itemMessage)
        {

            if (!loaded)
            {
                loadActionBuffer.Enqueue(() =>
                {
                    localRepo.Update(itemMessage.Payload).Wait();
                    OnUpdated?.Invoke(localRepo.Read(itemMessage.Payload.ID).Result, itemMessage.Payload);
                });
                return;
            }
            loadedTrigger.WaitOne();
            T previous =
                itemMessage.EndpointId == Self ?
                    updateRequests[itemMessage.MessageId].Message.Payload :
                    localRepo.Read(itemMessage.Payload.ID).Result;
            localRepo.Update(itemMessage.Payload).Wait();
            OnUpdated?.Invoke(previous, itemMessage.Payload);
            if (itemMessage.EndpointId == Self)
            {
                var record = updateRequests[itemMessage.MessageId];
                record.Message.Payload = itemMessage.Payload;
                record.Recieved.Set();
                updateRequests.Remove(record);
            }
        }

        private void recieveItemInitialLoad(Message<T> itemMessage)
        {
            localRepo.Write(itemMessage.Payload).Wait();
            OnCreated?.Invoke(itemMessage.Payload);
        }

        private void recieveLoaded(Message<Guid> confirmation)
        {
            if (confirmation.Payload != _self)
                throw new InvalidOperationException("Client finished loading before id set");
            loaded = true;
            foreach (var action in loadActionBuffer)
                action();
            loadedTrigger.Set();
        }

        private void recieveRepoId(Message<Guid> guidMessage)
        {
            _self = guidMessage.Payload;
            recieveRepoIdTrigger.Set();
        }

        public async Task<int> CreateTable() =>
            throw new NotSupportedException();

        public async Task<int> DropTable()
            => throw new NotSupportedException();

        public Task<IEnumerable<T>> Read()
        {
            loadedTrigger.WaitOne();
            throw new NotImplementedException();
        }

        public async Task<T> Read(int id)
        {
            loadedTrigger.WaitOne();
            return await localRepo.Read(id);
        }

        public async Task<IEnumerable<T>> Read(Expression<Func<T, bool>> predicate)
        {
            loadedTrigger.WaitOne();
            return await localRepo.Read(predicate);
        }

        public async Task<IEnumerable<T>> Read<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
        {
            loadedTrigger.WaitOne();
            return await localRepo.Read(predicate, param);
        }

        public async Task<int> Write(params T[] values)
        {
            loadedTrigger.WaitOne();

            foreach (var item in values)
            {
                var record = writeRequests.Next(new Message<T>
                {
                    EndpointId = Self,
                    Payload = item,
                });
                await hub.SendAsync(ServerAddItem, record.Message);
                record.Wait();
                item.ID = record.Message.Payload.ID;
            }
            return values.Length;
        }

        public async Task<int> Update(params T[] values)
        {
            loadedTrigger.WaitOne();
            foreach (var item in values)
            {
                var record = updateRequests.Next(new Message<T>
                {
                    EndpointId = Self,
                    Payload = item,
                });
                await hub.SendAsync(ServerUpdateItem, record.Message);
                record.Wait();
            }
            return values.Length;
        }

        public async Task<int> Delete(params int[] ids)
        {
            loadedTrigger.WaitOne();
            foreach (var id in ids)
            {
                var record = deleteRequests.Next(new Message<int>
                {
                    EndpointId = Self,
                    Payload = id,
                });
                await hub.SendAsync(ServerDeleteItem, record.Message);
                record.Wait();
            }
            return ids.Length;
        }

        public async Task<int> Delete(Expression<Func<T, bool>> predicate)
        {
            var test = predicate.Compile();
            loadedTrigger.WaitOne();
            return await Delete((await localRepo.Read()).Where(test).Select(i => i.ID).ToArray());
        }

        public async Task<int> Delete<TParam>(Expression<Func<T, TParam, bool>> predicate, TParam param)
        {
            var test = predicate.Compile();
            loadedTrigger.WaitOne();
            return await Delete((await localRepo.Read()).Where(i => test(i, param)).Select(i => i.ID).ToArray());
        }
    }
}

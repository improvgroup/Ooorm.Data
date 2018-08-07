using Microsoft.AspNetCore.SignalR;
using Ooorm.Data.SignalrClient;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Ooorm.Data.SignalrHub
{
    public class OoormHub<T> : Hub where T : IDbItem
    {
        private readonly Guid Self = Guid.NewGuid();

        private readonly IDatabase BackingDb;

        public OoormHub(IDatabase db) => BackingDb = db;


        private Message<TPayload> ServerMessage<TPayload>(TPayload payload)
        {
            return new Message<TPayload>
            {
                OriginIsServer = true,
                EndpointId = Self,
                Payload = payload
            };
        }

        private static readonly ConcurrentDictionary<Guid, IClientProxy> loadingClients = new ConcurrentDictionary<Guid, IClientProxy>();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var clientId = Guid.NewGuid();
            await Clients.Caller.SendAsync(SignalrRepository<T>.ClientSetId, ServerMessage(clientId));
            foreach (var item in await BackingDb.Read<T>())
                await Clients.Caller.SendAsync(SignalrRepository<T>.ClientLoadItem, ServerMessage(item));
            await Clients.Caller.SendAsync(SignalrRepository<T>.ClientLoaded, ServerMessage(clientId));
        }

        public async Task ServerAddItem(Message<T> message)
        {
            await BackingDb.Write(message.Payload);
            await Clients.All.SendAsync(SignalrRepository<T>.ClientRecieveItemAdded, message);
        }

        public async Task ServerDeleteItem(Message<int> message)
        {
            await BackingDb.Delete<T>(message.Payload);
            await Clients.All.SendAsync(SignalrRepository<T>.ClientRecieveItemDeleted, message);
        }

        public async Task ServerUpdateItem(Message<T> message)
        {
            await BackingDb.Update(message.Payload);
            await Clients.All.SendAsync(SignalrRepository<T>.ClientRecieveItemUpdated, message);
        }
    }
}

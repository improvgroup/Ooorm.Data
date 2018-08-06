using Microsoft.AspNetCore.SignalR;
using Ooorm.Data.SignalrClient;
using System;
using System.Threading.Tasks;

namespace Ooorm.Data.SignalrHub
{
    public class OoormHub<T> : Hub where T : IDbItem
    {
        private readonly IDatabase BackingDb;

        public OoormHub(IDatabase db) => BackingDb = db;


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task ReadItem(int id)
        {
            throw new NotImplementedException();
        }

        public async Task ReadItemRange(int firstId, int lastId)
        {
            throw new NotImplementedException();
        }



        public async Task CreateItem(T item)
        {
            await BackingDb.Write(item);
            //await Clients.All.SendAsync(SignalrDatabase.ClientRecieveItemAdded, item);
        }

        public async Task DeleteItem(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateItem(T item)
        {
            throw new NotImplementedException();
        }
    }
}

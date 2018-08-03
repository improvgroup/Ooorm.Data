using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace Ooorm.Data.SignalrHub
{
    public class OoormHubBase<T> : Hub where T : IDbItem
    {
        protected OoormHubBase(ICrudRepository<T> repo)
        {

        }

        public async Task Created(T item)
        {

        }
    }
}

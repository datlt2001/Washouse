using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Web.Hub
{
    public class MessageHub : Hub<IMessageHubClient>
    {
        public async Task NotifyToUser(string message)
        {
            /*string clientId = Context.ConnectionId;
            await Clients.Clients(clientId).NotifyToUser(message);*/
            await Clients.All.NotifyToUser(message);
        }
    }
}

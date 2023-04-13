using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Web.Hubs
{
    public class MessageHub : Hub
    {
        public async Task NotifyToUser(string message)
        {
            /*string clientId = Context.ConnectionId;
            await Clients.Clients(clientId).NotifyToUser(message);*/
            await Clients.All.SendAsync(message);
        }

        public async Task SendNotification(int accountId, string message)
        {
            var user = Context.UserIdentifier; // get the user ID of the current connection

            if (user != null && user == accountId.ToString())
            {
                await Clients.User(user).SendAsync(message);
            }
        }

    }
}

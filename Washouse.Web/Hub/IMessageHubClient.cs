using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Washouse.Model.Models;

namespace Washouse.Web.Hub
{
    public interface IMessageHubClient
    {
        Task NotifyToUser(string message);
        Task ReceiveNotification(string message);
    }
}

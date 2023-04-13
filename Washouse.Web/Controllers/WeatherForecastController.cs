//using GoogleMaps.LocationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NuGet.Packaging.Signing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Service;
using Washouse.Service.Interface;
using Washouse.Web.Hubs;

namespace Washouse.Web.Controllers
{
    [Route("api/Test")]
    public class TestController : Controller
    {
        private IHubContext<MessageHub> _signalrHub;

        public TestController(IHubContext<MessageHub> signalrHub)
        {
            _signalrHub = signalrHub;
        }

        [HttpPost]
        public async Task<string> Post()
        {
            var retMessage = string.Empty;
            try
            {
                //msg.Timestamp = Timestamp.UtcNow.ToString();
                await _signalrHub.Clients.All.SendAsync("haha");
                retMessage = "Success";
            }
            catch (Exception e)
            {
                retMessage = e.ToString();
            }
            return retMessage;
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using static Google.Apis.Requests.BatchRequest;
using Washouse.Web.Models;
using Washouse.Model.Models;
using Washouse.Model.ResponseModels;
using System.Collections.Generic;

namespace Washouse.Web.Controllers
{
    [Route("api/wards")]
    [ApiController]
    public class WardController : ControllerBase
    {
        #region Initialize
        private readonly IWardService _wardService;

        public WardController(IWardService wardService)
        {
            this._wardService = wardService;
        }
        #endregion

        
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;

namespace Washouse.Web.Controllers
{
    [Route("api/request/service")]
    [ApiController]
    public class ServiceRequestController : ControllerBase
    {
        #region Initialize
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IServiceService _serviceService;

        public ServiceRequestController(IServiceRequestService serviceRequestService, 
            IServiceService serviceService)
        {
            this._serviceRequestService = serviceRequestService;
            this._serviceService = serviceService;
        }
        #endregion

        [HttpPost("updateService")]
        public async Task<IActionResult> Update([FromForm]ServiceRequestModel serviceRequestmodel, int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    ServiceRequest serviceRequest = new ServiceRequest();
                    var service = await _serviceService.GetById(id);
                    if (service == null)
                    {
                        return NotFound();
                    } else
                    {
                        serviceRequest.Id = 0;
                        serviceRequest.ServiceRequesting = service.Id;
                        serviceRequest.RequestStatus = true;
                        serviceRequest.ServiceName = serviceRequestmodel.ServiceName;
                        serviceRequest.Alias = serviceRequestmodel.Alias;
                        serviceRequest.CategoryId = serviceRequestmodel.CategoryId;
                        serviceRequest.Description = serviceRequestmodel.Description;
                        serviceRequest.PriceType = serviceRequestmodel.PriceType;
                        serviceRequest.Image = serviceRequestmodel.Image;
                        if (!serviceRequest.PriceType)
                        {
                            serviceRequest.Price = serviceRequestmodel.Price;
                        }
                        serviceRequest.TimeEstimate = serviceRequestmodel.TimeEstimate;
                        serviceRequest.Status = service.Status;
                        serviceRequest.HomeFlag = service.HomeFlag;
                        serviceRequest.HotFlag = service.HotFlag;
                        serviceRequest.Rating = service.Rating;
                        serviceRequest.CreatedDate = service.CreatedDate;
                        serviceRequest.UpdatedDate = DateTime.Now;
                        var result = _serviceRequestService.Add(serviceRequest);
                        return Ok(result);
                    }
                }
                else { return BadRequest(); }
            }
            catch
            {
                    return BadRequest();
            }
        }

    }
}

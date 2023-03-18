using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;

namespace Washouse.Web.Controllers
{
    [ApiController]
    [Route("api/service")]
    public class ServiceController : ControllerBase
    {
        #region Initialize
        private readonly ICenterService _centerService;
        private readonly IServiceService _serviceService;

        public ServiceController(ICenterService centerService, IServiceService serviceService)
        {
            this._centerService = centerService;
            this._serviceService = serviceService;
        }

        #endregion

        //[Route("GetServicesOfACenter")]
        [HttpGet("getServicesOfACenter/{id}")]
        public async Task<IActionResult> GetServicesOfACenter(int id)
        {
            try
            {
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    var services = _serviceService.GetAll().Result.Where(a => a.CenterId == id);
                    if (services != null)
                    {
                        return Ok(services);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch {
                return BadRequest();
            }
        }

        //[Route("Details/{id}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var services = await _serviceService.GetById(id);
                if (services != null)
                {
                    return Ok(services);
                }
                else
                {
                    return NotFound();
                }
            }
            catch {
                return BadRequest();
            }
        }

        [HttpPut("deactivateService/{id}")]
        public async Task<IActionResult> DeactivateService(int id)
        {

            try
            {
                var service = await _serviceService.GetById(id);
                if (service == null)
                {
                    return NotFound();
                }
                await _serviceService.DeactivateService(id);
                return Ok();
            } catch {   
                return BadRequest();
            }
            
        }

        [HttpPost("createService")]
        public async Task<IActionResult> CreateService([FromForm] ServiceRequestModel serviceRequestmodel)
        {
            try
            {
                Model.Models.Service serviceRequest = new Model.Models.Service();
                if (ModelState.IsValid)
                {
                    serviceRequest.Id = 0;
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
                    serviceRequest.Status = "addRequest";
                    serviceRequest.HomeFlag = false;
                    serviceRequest.HotFlag = false;
                    serviceRequest.Rating = 0;
                    serviceRequest.CreatedDate = DateTime.Now;
                    serviceRequest.UpdatedDate = DateTime.Now;
                    //serviceRequest.CenterId = 
                    var result = _serviceService.Add(serviceRequest);
                    return Ok(result);
                }
                else { return BadRequest(); }
            }
            catch {
                return BadRequest();
            }
        }
    }
}

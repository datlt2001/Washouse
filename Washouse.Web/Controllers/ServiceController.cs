using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Washouse.Service.Interface;
using Washouse.Web.Infrastructure;

namespace Washouse.Web.Controllers
{
    [ApiController]
    [Route("api/service")]
    public class ServiceController : ControllerBase
    {
        #region Initialize
        private ICenterService _centerService;
        private IServiceService _serviceService;
        private ErrorLogger _errorLogger;

        public ServiceController(ICenterService centerService, IServiceService serviceService, ErrorLogger errorLogger)
        {
            this._centerService = centerService;
            this._serviceService = serviceService;
            this._errorLogger = errorLogger;
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
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex);
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
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex);
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
            } catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex);
                return BadRequest();
            }
            
        }
    }
}

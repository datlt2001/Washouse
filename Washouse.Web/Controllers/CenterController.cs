using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Service.Interface;
using Washouse.Web.Infrastructure;

namespace Washouse.Web.Controllers
{
    [Route("api/center")]
    [ApiController]
    public class CenterController : ControllerBase
    {
        #region Initialize
        private ICenterService _centerService;
        //private IErrorService _errorService;
        private ErrorLogger _errorLogger;

        //public CenterController(ICenterService centerService, IErrorService errorService)
        public CenterController(ICenterService centerService, ErrorLogger errorLogger)
        //public CenterController(ICenterService centerService)
        {
            this._centerService = centerService;
            this._errorLogger = errorLogger;
        }

        #endregion

        [Route("getAll")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var centerList = await _centerService.GetAll();
                return Ok(centerList);
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
                var center = await _centerService.GetById(id);
                if (center != null)
                {
                    return Ok(center);
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

        [HttpGet("search")]
        public async Task<IActionResult> Search(string searchKey, int page, int pageSize)
        {
            int totalRow = 0;
            try
            {
                var centerList = _centerService.GetAllBySearchKeyPaging(searchKey, page, pageSize, out totalRow);
                return Ok(centerList);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogErrorAsync(ex);
                return BadRequest();
            }
        }
    }
}

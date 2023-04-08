using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.Models;
using Microsoft.AspNetCore.Http;
using Washouse.Model.ResponseModels.ManagerResponseModel;
using System.Collections.Generic;

namespace Washouse.Web.Controllers
{
    [Route("api/promotions")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
       private IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public IActionResult GetPromotionList()
        {
            var promotion = _promotionService.GetAll();
            if (promotion == null) { return NotFound(); }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = promotion
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPromotionById(int id)
        {
            var promotion = await _promotionService.GetById(id);
            if (promotion == null) { return NotFound(); }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = promotion
            });
        }

        [HttpGet("center/{id}")]
        public  IActionResult GetPromotionByCenterId(int id)
        {
            var promotion = _promotionService.GetAllByCenterId(id);
            if (promotion == null) return NotFound();
            var promotionResponses = new List<PromotionCenterModel>();
            foreach (var item in promotion)
            {
                string _startDate = null;
                string _expireDate = null;
                string _updatedDate = null;
                if (item.StartDate.HasValue)
                {
                    _startDate = item.StartDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                }
                if (item.ExpireDate.HasValue)
                {
                    _expireDate = item.ExpireDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                }
                if (item.UpdatedDate.HasValue)
                {
                    _updatedDate = item.UpdatedDate.Value.ToString("dd-MM-yyyy HH:mm:ss");
                }

                var itemResponse = new PromotionCenterModel
                {
                    Code = item.Code,
                    Description = item.Description,
                    Discount = item.Discount,
                    StartDate = _startDate,
                    ExpireDate = _expireDate,
                    CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                    UpdatedDate = _updatedDate,
                    UseTimes = item.UseTimes
                };
                promotionResponses.Add(itemResponse);
            }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = promotionResponses
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                var promotion = new Promotion()
                {
                    Code = Input.Code,
                    Description = Input.Description,
                    Discount = Input.Discount,
                    StartDate = Input.StartDate,
                    ExpireDate= Input.ExpireDate,
                    UseTimes  = Input.UseTimes,
                    CenterId = Input.CenterId,
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now,


                };
                await _promotionService.Add(promotion);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = promotion
                });
            }
            else { return BadRequest(); }

        }

        [HttpGet("code/{code}")]
        public IActionResult GetPromotionByCode(string code)
        {
            var dis = _promotionService.GetDiscountByCode(code);
            if (dis == 0.0M) 
            return BadRequest(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Wrong Code",
                Data = null
            });
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = dis
            });
        }
    }
}

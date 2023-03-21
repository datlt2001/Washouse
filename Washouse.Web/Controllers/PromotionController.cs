﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.Models;
using Microsoft.AspNetCore.Http;

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
        public IActionResult GetPrmotionList()
        {
            var promotion = _promotionService.GetAll();
            if (promotion == null) { return NotFound(); }
            return Ok(promotion);
        }

        [HttpGet("{id}")]
        public IActionResult GetParentCategoryList(int id)
        {
            var promotion = _promotionService.GetById(id);
            if (promotion == null) { return NotFound(); }
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = promotion
            });
        }

        [HttpPost("add-promotion")]
        public async Task<IActionResult> Create([FromForm] PromotionRequestModel Input)
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
    }
}

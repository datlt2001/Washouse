﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using System.Linq;
using Washouse.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Washouse.Web.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        public IFeedbackService _feedbackService;
        public IAccountService _accountService;

        public FeedbackController(IFeedbackService feedbackService, IAccountService accountService)
        {
            _feedbackService = feedbackService;
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FeedbackRequestModel Input, int? centerId, int? orderDetailId)
        {
            if (ModelState.IsValid)
            {
                //var account = _accountService.GetById(userid);
                //var idlist = _feedbackService.GetIDList();
                //int lastId = idlist.Last();
                if (centerId == 0) centerId = null;
                if (orderDetailId == 0) orderDetailId = null;
                var feedback = new Feedback()
                {
                    Content = Input.Content,
                    Rating = Input.Rating,
                    OrderDetailId = orderDetailId,
                    CenterId = centerId,
                    CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                    CreatedDate = DateTime.Now

                };
                await _feedbackService.Add(feedback);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = feedback
                });
            }
            else { return BadRequest(); }

        }

        [HttpPost("reply")]
        public async Task<IActionResult> ReplyFeedback([FromBody] ReplyFeedbackRequestModel Input,
                                                                    int? centerId, int? orderDetailId, int FbId)
        {
            if (ModelState.IsValid)
            {
                //var account = _accountService.GetById(userid);
                if (centerId == 0) centerId = null;
                if (orderDetailId == 0) orderDetailId = null;
                Feedback existingFB = await _feedbackService.GetById(FbId);


                existingFB.ReplyDate = DateTime.Now;
                existingFB.ReplyBy = User.FindFirst(ClaimTypes.Email)?.Value;
                existingFB.ReplyMessage = Input.ReplyMessage;
                await _feedbackService.Update(existingFB);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = existingFB
                });
            }
            else { return BadRequest(); }

        }

        [HttpGet("centers/{id}")]
        public IActionResult GetFeedbackByCenterId(int centerId)
        {
            var feedbacks = _feedbackService.GetAllByCenterId(centerId);
            if (feedbacks == null) return NotFound();
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = feedbacks
            });
        }

        [HttpGet("order-details/{id}")]
        public IActionResult GetAllByOrderDetailId(int orderdetailid)
        {
            var feedbacks = _feedbackService.GetAllByOrderDetailId(orderdetailid);
            if (feedbacks == null) return NotFound();
            return Ok(new ResponseModel
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "success",
                Data = feedbacks
            });
        }
    }
}

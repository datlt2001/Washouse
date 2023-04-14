using Microsoft.AspNetCore.Mvc;
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
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Authorization;

namespace Washouse.Web.Controllers
{
    [Route("api/feedbacks")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        public readonly IFeedbackService _feedbackService;
        public readonly IAccountService _accountService;
        public readonly IOrderService _orderService;
        public readonly IServiceService _serviceService;
        public readonly ICustomerService _customerService;
        public readonly ICenterService _centerService;

        public FeedbackController(IFeedbackService feedbackService, IAccountService accountService,
             IServiceService serviceService, IOrderService orderService, ICustomerService customerService,
            ICenterService centerService)
        {
            _feedbackService = feedbackService;
            _accountService = accountService;
            _orderService = orderService;
            _serviceService = serviceService;
            _customerService = customerService;
            _centerService = centerService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("orders")]
        public async Task<IActionResult> FeedbackOrder([FromBody] FeedbackOrderRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Rating < 1 || model.Content == null)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Model is not valid",
                            Data = null
                        });
                    }
                    var order = await _orderService.GetOrderById(model.OrderId);
                    if (order == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found order",
                            Data = ""
                        });
                    }
                    if (!order.Status.Trim().ToLower().Equals("completed"))
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Only accept feedback when order completed",
                            Data = null
                        });
                    }
                    var customer = await _customerService.GetCustomerByAccID(int.Parse(User.FindFirst("Id")?.Value));

                    if (order.CustomerId != customer.Id)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "You are not order owner",
                            Data = null
                        });
                    }
                    var feedback = new Feedback()
                    {
                        Content = model.Content,
                        Rating = model.Rating,
                        //OrderDetailId = orderDetailId,
                        CenterId = model.CenterId,
                        OrderId = model.OrderId,
                        CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                        CreatedDate = DateTime.Now

                    };
                    await _feedbackService.Add(feedback);
                    var center = await _centerService.GetById(model.CenterId);
                    center.Rating = (center.Rating * center.NumOfRating + model.Rating) / (center.NumOfRating + 1);
                    center.NumOfRating = center.NumOfRating + 1;
                    await _centerService.Update(center);
                    order.IsFeedback = true;
                    await _orderService.Update(order);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = feedback
                    });
                }
                else
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Model is not valid",
                        Data = null
                    });
                }
            } catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("services")]
        public async Task<IActionResult> FeedbackService([FromBody] FeedbackServiceRequestModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.Rating < 1 || model.Content == null)
                    {
                        return BadRequest(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Model is not valid",
                            Data = null
                        });
                    }
                    var service = await _serviceService.GetById(model.ServiceId);
                    if (service == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status404NotFound,
                            Message = "Not found service",
                            Data = ""
                        });
                    }
                    var customer = await _customerService.GetCustomerByAccID(int.Parse(User.FindFirst("Id")?.Value));

                    var feedback = new Feedback()
                    {
                        Content = model.Content,
                        Rating = model.Rating,
                        //OrderDetailId = orderDetailId,
                        CenterId = model.CenterId,
                        ServiceId = model.ServiceId,
                        CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value,
                        CreatedDate = DateTime.Now

                    };
                    await _feedbackService.Add(feedback);
                    var center = await _centerService.GetById(model.CenterId);
                    center.Rating = (center.Rating * center.NumOfRating + model.Rating) / (center.NumOfRating + 1);
                    center.NumOfRating = center.NumOfRating + 1;
                    await _centerService.Update(center);
                    service.Rating = (service.Rating * service.NumOfRating + model.Rating) / (service.NumOfRating + 1);
                    service.NumOfRating = service.NumOfRating + 1;
                    await _serviceService.Update(service);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = feedback
                    });
                }
                else
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Model is not valid",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [Authorize(Roles = "Manager,Staff")]
        [HttpPost("reply")]
        public async Task<IActionResult> ReplyFeedback([FromBody] ReplyFeedbackRequestModel Input, int feedbackId)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Feedback existingFeedback = await _feedbackService.GetById(feedbackId);
                    if (existingFeedback == null)
                    {
                        return NotFound(new ResponseModel
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            Message = "Not found feedback",
                            Data = ""
                        });
                    }

                    existingFeedback.ReplyDate = DateTime.Now;
                    existingFeedback.ReplyBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    existingFeedback.ReplyMessage = Input.ReplyMessage;
                    await _feedbackService.Update(existingFeedback);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = existingFeedback
                    });
                }
                else
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Model is not valid",
                        Data = null
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpGet("centers/{centerId}")]
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

        [HttpGet("order-details/{orderDetailId}")]
        public IActionResult GetAllByOrderDetailId(int orderDetailId)
        {
            var feedbacks = _feedbackService.GetAllByOrderDetailId(orderDetailId);
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

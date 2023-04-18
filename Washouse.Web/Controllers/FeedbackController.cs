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
using System.Collections.Generic;
using Washouse.Model.ResponseModels;
using Washouse.Model.ViewModel;

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
        public async Task<IActionResult> GetFeedbackByCenterId(int centerId, [FromQuery] PaginationViewModel filter)
        {
            var center = await _centerService.GetById(centerId);
            if (center == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found center",
                    Data = ""
                });
            }
            var feedbacks = _feedbackService.GetAllByCenterId(centerId);
            if (feedbacks == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Center not have any feedback",
                    Data = null
                });
            }
            else
            {
                var feedbackResponses = new List<FeedbackResponseModel>();
                foreach (var item in feedbacks)
                {
                    string centerName, serviceName;
                    if (item.CenterId != null)
                    {
                        centerName = item.Center.CenterName;
                    }
                    else centerName = null;
                    if (item.ServiceId != null)
                    {
                        serviceName = item.Service.ServiceName;
                    }
                    else serviceName = null;
                    feedbackResponses.Add(new FeedbackResponseModel
                    {
                        Id = item.Id,
                        Content = item.Content,
                        Rating = item.Rating,
                        OrderId = item.OrderId,
                        CenterId = item.CenterId,
                        CenterName = centerName,
                        ServiceId = item.ServiceId,
                        ServiceName = serviceName,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                        ReplyMessage = item.ReplyMessage,
                        ReplyBy = item.ReplyBy,
                        ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : null
                    });
                }
                int totalItems = feedbackResponses.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);
                feedbackResponses = feedbackResponses.Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize).ToList();
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = new
                    {
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        ItemsPerPage = filter.PageSize,
                        PageNumber = filter.Page,
                        Items = feedbackResponses
                    }
                });
            }
        }

        [HttpGet("services/{serviceId}")]
        public async Task<IActionResult> GetAllByServiceId(int serviceId, [FromQuery] PaginationViewModel filter)
        {
            var service = await _serviceService.GetById(serviceId);
            if (service == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Not found service",
                    Data = null
                });
            }
            var feedbacks = _feedbackService.GetAllByServiceId(serviceId);
            if (feedbacks == null)
            {
                return NotFound(new ResponseModel
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Service not have any feedback",
                    Data = null
                });
            }
            else
            {
                var feedbackResponses = new List<FeedbackResponseModel>();
                foreach (var item in feedbacks)
                {
                    string centerName, serviceName;
                    if (item.CenterId != null)
                    {
                        centerName = item.Center.CenterName;
                    }
                    else centerName = null;
                    if (item.ServiceId != null)
                    {
                        serviceName = item.Service.ServiceName;
                    }
                    else serviceName = null;
                    feedbackResponses.Add(new FeedbackResponseModel
                    {
                        Id = item.Id,
                        Content = item.Content,
                        Rating = item.Rating,
                        OrderId = item.OrderId,
                        CenterId = item.CenterId,
                        CenterName = centerName,
                        ServiceId = item.ServiceId,
                        ServiceName = serviceName,
                        CreatedBy = item.CreatedBy,
                        CreatedDate = item.CreatedDate.ToString("dd-MM-yyyy HH:mm:ss"),
                        ReplyMessage = item.ReplyMessage,
                        ReplyBy = item.ReplyBy,
                        ReplyDate = item.ReplyDate.HasValue ? item.ReplyDate.Value.ToString("dd-MM-yyyy HH:mm:ss") : null
                    });
                }
                int totalItems = feedbackResponses.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);
                feedbackResponses = feedbackResponses.Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize).ToList();
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = new
                    {
                        TotalItems = totalItems,
                        TotalPages = totalPages,
                        ItemsPerPage = filter.PageSize,
                        PageNumber = filter.Page,
                        Items = feedbackResponses
                    }
                });
            }
        }
    }
}

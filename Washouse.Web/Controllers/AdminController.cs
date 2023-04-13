using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Twilio.Http;
using Washouse.Model.ResponseModels.AdminResponseModel;
using Washouse.Common.Helpers;
using Washouse.Service.Implement;
using Washouse.Model.Models;
using System.Globalization;
using static Google.Apis.Requests.BatchRequest;

namespace Washouse.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        #region Initialize

        private readonly IAccountService _accountService;
        private readonly ICenterService _centerService;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly ILocationService _locationService;
        private readonly IWardService _wardService;
        private readonly IOperatingHourService _operatingHourService;
        private readonly IServiceService _serviceService;
        private readonly IStaffService _staffService;
        private readonly ICenterRequestService _centerRequestService;
        private readonly IFeedbackService _feedbackService;
        private readonly IPostService _postService;
        private readonly IServiceCategoryService _serviceCategoryService;

        public AdminController(ICenterService centerService, ICloudStorageService cloudStorageService,
            IAccountService accountService,
            ILocationService locationService, IWardService wardService,
            IOperatingHourService operatingHourService, IServiceService serviceService,
            IStaffService staffService, ICenterRequestService centerRequestService, IFeedbackService feedbackService,
            IPostService postService, IServiceCategoryService serviceCategoryService)
        {
            this._centerService = centerService;
            this._accountService = accountService;
            this._locationService = locationService;
            this._cloudStorageService = cloudStorageService;
            this._wardService = wardService;
            this._operatingHourService = operatingHourService;
            this._serviceService = serviceService;
            this._staffService = staffService;
            this._centerRequestService = centerRequestService;
            this._serviceCategoryService = serviceCategoryService;
            this._feedbackService = feedbackService;
            this._postService = postService;
        }

        #endregion

        // GET: api/admin/centers
        [HttpGet("centers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllCenters(
            [FromQuery] AdminFilterCentersRequestModel filterCentersRequestModel)
        {
            try
            {
                var centerList = await _centerService.GetAll();
                if (filterCentersRequestModel.SearchString != null)
                {
                    centerList = centerList.Where(res =>
                        res.CenterName.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower())
                        || (res.Alias != null &&
                            res.Alias.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower()))
                    ).ToList();
                }

                if (filterCentersRequestModel.Status != null)
                {
                    centerList = centerList.Where(res =>
                        res.Status.Trim().ToLower().Equals(filterCentersRequestModel.Status.Trim().ToLower())).ToList();
                }

                var response = new List<AdminCenterResponseModel>();
                foreach (var center in centerList)
                {
                    var staffs = _staffService.GetAllByCenterId(center.Id);
                    var manager = staffs.FirstOrDefault(st => (st.IsManager != null && (bool)st.IsManager == true));
                    var account = new Account();
                    if (manager != null)
                    {
                        account = await _accountService.GetById(manager.AccountId);
                    }

                    response.Add(new AdminCenterResponseModel
                    {
                        Id = center.Id,
                        Thumbnail = center.Image != null
                            ? await _cloudStorageService.GetSignedUrlAsync(center.Image)
                            : null,
                        Title = center.CenterName,
                        Alias = center.Alias,
                        Rating = center.Rating,
                        NumOfRating = center.NumOfRating,
                        Phone = center.Phone,
                        Status = center.Status,
                        TaxCode = center.TaxCode.Trim(),
                        ManagerId = manager != null ? manager.Id : null,
                        ManagerName = manager != null ? account.FullName : null,
                        CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName + ", " +
                                        center.Location.Ward.District.DistrictName
                    });
                }

                int totalItems = response.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filterCentersRequestModel.PageSize);

                response = response.Skip((filterCentersRequestModel.Page - 1) * filterCentersRequestModel.PageSize)
                    .Take(filterCentersRequestModel.PageSize).ToList();

                if (response.Count != 0)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            ItemsPerPage = filterCentersRequestModel.PageSize,
                            PageNumber = filterCentersRequestModel.Page,
                            Items = response
                        }
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
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

        /// <summary>
        ///   -POST: 
        /// type: ServiceCategory, Promotion, Event, System
        /// type-description: Service Category posts, Promotion posts, Event posts, System updates
        /// status : Draft, Pending, Published, Private, Scheduled
        /// </summary>
        /// <param name="filterPostModel"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("posts")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAllPosts([FromQuery] AdminFilterPostRequestModel filterPostModel)
        {
            try
            {
                var post = _postService.GetAll();
                if (post.Count() == 0)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
                        Data = ""
                    });
                }

                if (filterPostModel.Type != null)
                {
                    post = post.Where(p => p.Type.Trim().ToLower().Equals(filterPostModel.Type.ToLower().Trim()));
                }

                if (filterPostModel.Status != null)
                {
                    post = post.Where(p => p.Status.Trim().ToLower().Equals(filterPostModel.Status.ToLower().Trim()));
                }

                var response = new List<PostResponseModel>();
                foreach (var postItem in post)
                {
                    response.Add(new PostResponseModel
                    {
                        Id = postItem.Id,
                        Title = postItem.Title,
                        Content = postItem.Content,
                        Thumbnail = postItem.Thumbnail,
                        Type = postItem.Type,
                        Status = postItem.Status,
                        CreatedDate = (postItem.CreatedDate).ToString("dd-MM-yyyy HH:mm:ss"),
                        UpdatedDate = postItem.UpdateDate.HasValue
                            ? (postItem.UpdateDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                            : null
                    });
                }

                int totalItems = response.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filterPostModel.PageSize);
                response = response.Skip((filterPostModel.Page - 1) * filterPostModel.PageSize)
                    .Take(filterPostModel.PageSize).ToList();
                if (response.Count > 0)
                {
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            TotalItems = totalItems,
                            TotalPages = totalPages,
                            ItemsPerPage = filterPostModel.PageSize,
                            PageNumber = filterPostModel.Page,
                            Items = response
                        }
                    });
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
                        Data = ""
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Input"></param>
        /// <remarks>
        /// {
        ///   "title": "Đừng đi đâu xa, hãy đến với chúng tôi",
        ///   "content": "Đừng để thời gian quý báu của bạn phải trôi qua một cách vô ích. Hãy đến với dịch vụ của chúng tôi để cuộc sống dễ dàng hơn",
        ///   "type": "System",
        ///   "status": "Scheduled",
        ///   "publishTime": "14-04-2023 15:00:00" 
        ///   }
        /// </remarks>
        /// <returns>
        /// </returns>
        [HttpPost("posts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePost([FromBody] PostRequestModel Input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DateTime? UpdatedTime = null;
                    if (Input.Status.Trim().ToLower().Equals("scheduled"))
                    {
                        DateTime PublishTime;
                        if (!string.IsNullOrEmpty(Input.PublishTime) && DateTime.TryParseExact(Input.PublishTime,
                                "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                out PublishTime))
                        {
                            try
                            {
                                UpdatedTime = DateTime.ParseExact(Input.PublishTime, "dd-MM-yyyy HH:mm:ss",
                                    CultureInfo.InvariantCulture);
                            }
                            catch (FormatException ex)
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Publish time: " + ex.Message,
                                    Data = null
                                });
                                //Console.WriteLine("Failed to parse date: " + ex.Message);
                                // handle the parse failure
                            }
                        }
                        else
                        {
                            UpdatedTime = DateTime.Now;
                        }
                    }

                    var posts = new Post()
                    {
                        AuthorId = int.Parse(User.FindFirst("Id")?.Value),
                        Title = Input.Title,
                        Content = Input.Content,
                        Thumbnail = Input.SavedFileName,
                        Status = Input.Status,
                        Type = Input.Type,
                        CreatedDate = DateTime.Now,
                        UpdateDate = UpdatedTime,
                    };
                    await _postService.Add(posts);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            postId = posts.Id
                        }
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

        [HttpPut("posts")]
        public async Task<IActionResult> Update(int Id, [FromBody] UpdatePostRequestModel updatePost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Model is not valid",
                    Data = null
                });
            }
            else
            {
                //Category existingCategorySevice =  await _serviceCategoryService.GetById(id);
                Post existingPost = await _postService.GetById(Id);
                if (existingPost == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
                        Data = ""
                    });
                }
                else
                {
                    DateTime UpdatedTime = DateTime.Now;
                    if (updatePost.Status != null && updatePost.Status.Trim().ToLower().Equals("scheduled"))
                    {
                        DateTime PublishTime;
                        if (!string.IsNullOrEmpty(updatePost.PublishTime) && DateTime.TryParseExact(
                                updatePost.PublishTime, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture,
                                DateTimeStyles.None, out PublishTime))
                        {
                            try
                            {
                                UpdatedTime = DateTime.ParseExact(updatePost.PublishTime, "dd-MM-yyyy HH:mm:ss",
                                    CultureInfo.InvariantCulture);
                            }
                            catch (FormatException ex)
                            {
                                return BadRequest(new ResponseModel
                                {
                                    StatusCode = StatusCodes.Status400BadRequest,
                                    Message = "Publish time: " + ex.Message,
                                    Data = null
                                });
                                //Console.WriteLine("Failed to parse date: " + ex.Message);
                                // handle the parse failure
                            }
                        }
                        else
                        {
                            UpdatedTime = DateTime.Now;
                        }
                    }

                    existingPost.Title = updatePost.Title != null ? updatePost.Title : existingPost.Title;
                    existingPost.Content = updatePost.Content != null ? updatePost.Content : existingPost.Content;
                    existingPost.Thumbnail = updatePost.SavedFileName != null
                        ? updatePost.SavedFileName
                        : existingPost.Thumbnail;
                    existingPost.Type = updatePost.Type != null ? updatePost.Type : existingPost.Type;
                    existingPost.Status = updatePost.Status != null ? updatePost.Status : existingPost.Status;
                    existingPost.UpdateDate = UpdatedTime;

                    await _postService.Update(existingPost);
                    return Ok(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status200OK,
                        Message = "success",
                        Data = new
                        {
                            PostId = existingPost.Id,
                        }
                    });
                }
            }
        }
    }
}
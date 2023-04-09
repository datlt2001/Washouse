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

namespace Washouse.Web.Controllers
{
    [Authorize(Roles ="Admin")]
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
        public AdminController(ICenterService centerService, ICloudStorageService cloudStorageService, IAccountService accountService,
                                ILocationService locationService, IWardService wardService,
                                IOperatingHourService operatingHourService, IServiceService serviceService,
                                IStaffService staffService, ICenterRequestService centerRequestService, IFeedbackService feedbackService)
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
            this._feedbackService = feedbackService;
        }

        #endregion

        // GET: api/admin/centers
        [HttpGet("centers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAll([FromQuery] AdminFilterCentersRequestModel filterCentersRequestModel)
        {
            try
            {
                var centerList = await _centerService.GetAll();
                if (filterCentersRequestModel.SearchString != null)
                {
                    centerList = centerList.Where(res => res.CenterName.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower())
                                                  || (res.Alias != null && res.Alias.ToLower().Contains(filterCentersRequestModel.SearchString.ToLower()))
                                             ).ToList();
                }
                if (filterCentersRequestModel.Status != null)
                {
                    centerList = centerList.Where(res => res.Status.Trim().ToLower().Equals(filterCentersRequestModel.Status.Trim().ToLower())).ToList();
                }
                var response = new List<AdminCenterResponseModel>();
                foreach (var center in centerList)
                {
                    var staffs = _staffService.GetAllByCenterId(center.Id);
                    var manager = staffs.FirstOrDefault(st => (st.IsManager != null && (bool)st.IsManager == true));
                    var account = await _accountService.GetById(manager.AccountId);
                    response.Add(new AdminCenterResponseModel
                    {
                        Id = center.Id,
                        Thumbnail = center.Image != null ? await _cloudStorageService.GetSignedUrlAsync(center.Image) : null,
                        Title = center.CenterName,
                        Alias = center.Alias,
                        Rating = center.Rating,
                        NumOfRating = center.NumOfRating,
                        Phone = center.Phone,
                        Status = center.Status,
                        TaxCode = center.TaxCode.Trim(),
                        ManagerId = manager.Id,
                        ManagerName = account.FullName,
                        CenterAddress = center.Location.AddressString + ", " + center.Location.Ward.WardName + ", " + center.Location.Ward.District.DistrictName                        
                    });
                }
                
                int totalItems = response.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filterCentersRequestModel.PageSize);

                response = response.Skip((filterCentersRequestModel.Page - 1) * filterCentersRequestModel.PageSize).Take(filterCentersRequestModel.PageSize).ToList();

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

    }
}

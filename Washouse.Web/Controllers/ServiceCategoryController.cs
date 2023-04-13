﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Washouse.Common.Helpers;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;
using static Google.Apis.Requests.BatchRequest;
using static System.Net.Mime.MediaTypeNames;

namespace Washouse.Web.Controllers
{
    [Route("api/service-categories")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly IServiceCategoryService _serviceCategoryService;
        private readonly IServiceService _serviceService;
        private readonly ICloudStorageService _cloudStorageService;

        public ServiceCategoryController(
            IServiceCategoryService serviceCategoryService,
            IServiceService serviceService,
            ICloudStorageService cloudStorageService)
        {
            this._serviceCategoryService = serviceCategoryService;
            _serviceService = serviceService;
            this._cloudStorageService = cloudStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryList()
        {
            try
            {
                var categories = _serviceCategoryService.GetAll();
                if (categories == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Don't have any category.",
                        Data = null
                    });
                }

                var response = new List<CategoryResponseModel>();
                foreach (var item in categories)
                {
                    response.Add(new CategoryResponseModel
                    {
                        CategoryId = item.Id,
                        CategoryName = item.CategoryName,
                        CategoryAlias = item.Alias,
                        Description = item.Description,
                        HomeFlag = item.HomeFlag,
                        Image = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
                    });
                }

                return Ok(
                    new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = response
                    });
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var item = await _serviceCategoryService.GetById(id);
                if (item == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found category.",
                        Data = null
                    });
                }

                var response = new CategoryResponseModel
                {
                    CategoryId = item.Id,
                    CategoryName = item.CategoryName,
                    CategoryAlias = item.Alias,
                    Description = item.Description,
                    HomeFlag = item.HomeFlag,
                    Image = item.Image != null ? await _cloudStorageService.GetSignedUrlAsync(item.Image) : null,
                };
                return Ok(new ResponseModel
                {
                    StatusCode = 0,
                    Message = "success",
                    Data = response
                });
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequestModel Input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Category cate = new Category();
                    cate.CreatedDate = DateTime.Now;
                    cate.CreatedBy = User.FindFirst(ClaimTypes.Email)?.Value;
                    cate.CategoryName = Input.CategoryName;
                    cate.Status = Input.Status;
                    cate.Description = Input.Description;
                    cate.HomeFlag = Input.HomeFlag;
                    cate.Image = Input.SavedFileName;

                    await _serviceCategoryService.Add(cate);
                    var response = new CategoryResponseModel
                    {
                        CategoryId = cate.Id,
                        CategoryName = cate.CategoryName,
                        CategoryAlias = cate.Alias,
                        Description = cate.Description,
                        HomeFlag = cate.HomeFlag,
                        Image = cate.Image != null ? await _cloudStorageService.GetSignedUrlAsync(cate.Image) : null,
                    };
                    return Ok(new ResponseModel
                    {
                        StatusCode = 0,
                        Message = "success",
                        Data = response
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
    }
}
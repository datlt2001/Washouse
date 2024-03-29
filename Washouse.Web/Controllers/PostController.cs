﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private IPostService _postService;
        private readonly ICloudStorageService _cloudStorageService;

        public PostController(IPostService postService, ICloudStorageService cloudStorageService)
        {
            _postService = postService;
            _cloudStorageService = cloudStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPostList([FromQuery] FilterPostRequestModel filterPostModel)
        {
            try
            {
                var post = _postService.GetAll();
                post = post.Where(p => p.Status.Trim().ToLower().Equals("published"))
                    .OrderByDescending(p => p.UpdateDate ?? p.CreatedDate);
                
                if (filterPostModel.Type != null)
                {
                    post = post.Where(p => p.Type.Trim().ToLower().Equals(filterPostModel.Type.ToLower().Trim()));
                }

                int totalItems = post.Count();
                int totalPages = (int)Math.Ceiling((double)totalItems / filterPostModel.PageSize);
                post = post.Skip((filterPostModel.Page - 1) * filterPostModel.PageSize)
                    .Take(filterPostModel.PageSize).ToList();

                var response = new List<PostResponseModel>();
                foreach (var postItem in post)
                {
                    response.Add(new PostResponseModel
                    {
                        Id = postItem.Id,
                        Title = postItem.Title,
                        Content = postItem.Content,
                        Thumbnail =
                            postItem.Thumbnail != null
                                ? await _cloudStorageService.GetSignedUrlAsync(postItem.Thumbnail)
                                : null,
                        Type = postItem.Type,
                        Status = postItem.Status,
                        Description = postItem.Description,
                        CreatedDate = (postItem.CreatedDate).ToString("dd-MM-yyyy HH:mm:ss"),
                        UpdatedDate = postItem.UpdateDate.HasValue
                            ? (postItem.UpdateDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                            : null
                    });
                }

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

        [HttpGet("{PostId}")]
        public async Task<IActionResult> GetPostDetail(int PostId)
        {
            try
            {
                var postItem = await _postService.GetById(PostId);
                if (postItem == null)
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found",
                        Data = ""
                    });
                }

                if (!postItem.Status.Trim().ToLower().Equals("published"))
                {
                    return BadRequest(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "This post is not available now",
                        Data = null
                    });
                }

                var response = new PostResponseModel
                {
                    Id = postItem.Id,
                    Title = postItem.Title,
                    Content = postItem.Content,
                    Thumbnail =
                        postItem.Thumbnail != null
                            ? await _cloudStorageService.GetSignedUrlAsync(postItem.Thumbnail)
                            : null,
                    Type = postItem.Type,
                    Status = postItem.Status,
                    Description = postItem.Description,
                    CreatedDate = (postItem.CreatedDate).ToString("dd-MM-yyyy HH:mm:ss"),
                    UpdatedDate = postItem.UpdateDate.HasValue
                        ? (postItem.UpdateDate.Value).ToString("dd-MM-yyyy HH:mm:ss")
                        : null
                };
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
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
        public async Task<IActionResult> Create([FromBody] PostRequestModel Input)
        {
            if (ModelState.IsValid)
            {
                //int lastID = _postService.GetIDList().Last();
                var posts = new Post()
                {
                    AuthorId = int.Parse(User.FindFirst("Phone")?.Value),
                    Title = Input.Title,
                    Content = Input.Content,
                    Description = Input.Description,
                    //Thumbnail = await Utilities.UploadFile(Input.Thumbnail, @"images\post", Input.Thumbnail.FileName),
                    Status = "false",
                    Type = Input.Type,
                    CreatedDate = DateTime.Now,
                    //Id = lastID +1,
                };
                await _postService.Add(posts);
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                });
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("updatePost")]
        public async Task<IActionResult> Update(Post post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            else
            {
                //Category existingCategorySevice =  await _serviceCategoryService.GetById(id);
                Post existingCustomer = new Post();
                if (existingCustomer == null)
                {
                    return NotFound();
                }
                else
                {
                    post.Id = existingCustomer.Id;
                    existingCustomer = post;

                    await _postService.Update(existingCustomer);
                    return Ok(existingCustomer);
                }
            }
        }

        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> DeactivatePost(int id)
        {
            var post = await _postService.GetById(id);
            if (post == null)
            {
                return NotFound();
            }

            await _postService.DeactivatePost(id);
            return Ok();
        }

        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivatePost(int id)
        {
            var post = await _postService.GetById(id);
            if (post == null)
            {
                return NotFound();
            }

            await _postService.ActivatePost(id);
            return Ok();
        }
    }
}
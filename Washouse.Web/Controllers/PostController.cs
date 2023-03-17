using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Washouse.Model.Models;
using Washouse.Service.Implement;
using Washouse.Service.Interface;
using Microsoft.Extensions.Hosting;
using Washouse.Web.Models;
using Washouse.Common.Helpers;
using Washouse.Model.RequestModels;
using System.Linq;
using System.Collections.Generic;
using Washouse.Model.ResponseModels;
using Washouse.Web.Infrastructure;

namespace Washouse.Web.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private IPostService _postService;

        public PostController(IPostService postService)
        {
            this._postService = postService;
        }

        [HttpGet]
        public IActionResult GetPostList()
        {
            try { 
            var post = _postService.GetAll();
            post = post.OrderByDescending(p => p.CreatedDate)
                        .Where(p => p.Status == "true");
            var response = new List<PostResponseModel>();
            foreach (var postItem in post)
            {
                response.Add(new PostResponseModel
                {
                    Id = postItem.Id,
                    Title = postItem.Title,
                    Content = postItem.Content,
                    Thumbnail = postItem.Thumbnail,
                    CreatedDate = DateTime.Now,

                });
            }
            return Ok(response);
            }
            catch (Exception ex)
            {
                //await _errorLogger.LogErrorAsync(ex);
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _postService.GetById(id);
            if (post == null) { return NotFound(); }
            return Ok(post);
        }

        [HttpPost("addPost")]
        public async Task<IActionResult> Create([FromForm] PostRequestModel Input, int id)
        {
            if (ModelState.IsValid)
            {
                var posts = new Post()
                {
                    AuthorId = id,
                    Title= Input.Title,
                    Content= Input.Content,
                    Thumbnail = await Utilities.UploadFile(Input.Thumbnail, @"images\post", Input.Thumbnail.FileName),
                    Status = "false",
                    Type = Input.Type,
                    CreatedDate =  DateTime.Now,
                    //CreatedBy = "Admin",
                };
                await _postService.Add(posts);
                return Ok(posts);
            }
            else { return BadRequest(); }

        }

        [HttpPut("updatePost")]
        public async Task<IActionResult> Update(Post post)
        {
            if (!ModelState.IsValid) { return BadRequest(); }
            else
            {
                //Category existingCategorySevice =  await _serviceCategoryService.GetById(id);
                Post existingCustomer = new Post();
                if (existingCustomer == null) { return NotFound(); }
                else
                {
                    post.Id = existingCustomer.Id;
                    existingCustomer = post;

                    await _postService.Update(existingCustomer);
                    return Ok(existingCustomer);
                }


            }
        }

        [HttpPut("deactivatePost/{id}")]
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

        [HttpPut("activatePost/{id}")]
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


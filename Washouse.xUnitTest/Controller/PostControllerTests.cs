using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Controllers;
using Washouse.Web.Models;

namespace Washouse.xUnitTest.Controller
{
    
    public class PostControllerTests
    {
        private readonly IPostService _postService;
        private readonly ICloudStorageService _cloudStorageService;

        public PostControllerTests()
        {
            _postService = A.Fake<IPostService>();
            _cloudStorageService = A.Fake<ICloudStorageService>();
        }

        [Fact]
        public async Task GetPostList_WithFilter_ReturnsCorrectItemsPerPage()
        {
            // Arrange
            var filterPostModel = new FilterPostRequestModel
            {
                Page = 1,
                PageSize = 10,
                Type = "news"
            };
            var posts = A.CollectionOfDummy<Post>(5);
            var postList = new List<Post>
            {
            new Post { Id = 1, Type = "news", Status = "published", CreatedDate = DateTime.Now },
            new Post { Id = 2, Type = "news", Status = "published", CreatedDate = DateTime.Now },
            new Post { Id = 3, Type = "article", Status = "published", CreatedDate = DateTime.Now },
            new Post { Id = 4, Type = "news", Status = "published", CreatedDate = DateTime.Now },
            new Post { Id = 5, Type = "news", Status = "draft", CreatedDate = DateTime.Now },
            new Post { Id = 6, Type = "news", Status = "published", CreatedDate = DateTime.Now }
            };

            var fakePostService = A.Fake<IPostService>();
            A.CallTo(() => fakePostService.GetAll()).Returns(postList.AsQueryable());
            var fakeCloudStorageService = A.Fake<ICloudStorageService>();
            var controller = new PostController(fakePostService, fakeCloudStorageService);
            // Act
            var result = await controller.GetPostList(filterPostModel);
            

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(200, response.StatusCode);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetPostDetail_WithValidIdAndPublishedStatus_ReturnsOkObjectResult()
        {
            // Arrange
            int postId = 1;
            var post = new Post
            {
                Id = postId,
                Title = "Test Post",
                Content = "Test Content",
                Thumbnail = "test.jpg",
                Type = "test",
                Status = "Published",
                CreatedDate = new DateTime(2022, 1, 1),
                UpdateDate = new DateTime(2022, 1, 2)
            };
            var postService = A.Fake<IPostService>();
            A.CallTo(() => postService.GetById(postId)).Returns(post);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            //A.CallTo(() => cloudStorageService.GetSignedUrlAsync(post.Thumbnail)).Returns("https://test.com/test.jpg");
            var controller = new PostController(postService, cloudStorageService);

            // Act
            var result = await controller.GetPostDetail(postId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);

            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Equal("success", response.Message);

            var postResponse = Assert.IsType<PostResponseModel>(response.Data);
            Assert.Equal(post.Id, postResponse.Id);
            Assert.Equal(post.Title, postResponse.Title);
            Assert.Equal(post.Content, postResponse.Content);
            Assert.NotNull(postResponse.Thumbnail);
            Assert.Equal(post.Type, postResponse.Type);
            Assert.Equal(post.Status, postResponse.Status);
            Assert.Equal("01-01-2022 00:00:00", postResponse.CreatedDate);
            Assert.Equal("02-01-2022 00:00:00", postResponse.UpdatedDate);
        }

        [Fact]
        public async Task GetPostDetail_WithNonExistentId_ReturnsNotFoundObjectResult()
        {
            // Arrange
            int postId = 1;
            Post post = null;
            var postService = A.Fake<IPostService>();
            A.CallTo(() => postService.GetById(postId)).Returns(post);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            var controller = new PostController(postService, cloudStorageService);

            // Act
            var result = await controller.GetPostDetail(postId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(notFoundResult.Value);

            Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
            Assert.Equal("Not found", response.Message);
            Assert.Equal("", response.Data);
        }

        [Fact]
        public async Task Create_ValidInput_ReturnsOkObjectResult()
        {
            // Arrange
            var input = new PostRequestModel
            {
                Title = "Test Title",
                Content = "Test Content",
                Type = "Test Type",
                //Thumbnail = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Test Image Data")), 0, 0, "Test Image", "test.jpg")
            };
            var fakeUserIdentity = new ClaimsIdentity(new Claim[] { new Claim("Phone", "123456789") });
            var fakeUser = new ClaimsPrincipal(fakeUserIdentity);
            var controller = new PostController(_postService, _cloudStorageService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = fakeUser }
                }
            };

            // Act
            var result = await controller.Create(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ResponseModel>(okResult.Value);
            Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
            Assert.Equal("success", response.Message);
            var responseData = Assert.IsType<Post>(response.Data);
            Assert.Equal(int.Parse(fakeUserIdentity.FindFirst("Phone")?.Value), responseData.AuthorId);
            Assert.Equal(input.Title, responseData.Title);
            Assert.Equal(input.Content, responseData.Content);
            Assert.Equal(input.Type, responseData.Type);
            Assert.Equal("false", responseData.Status);
            
            
        }

        [Fact]
        public async Task Update_PostExists_ReturnsOkObjectResult()
        {
            // Arrange
            int postId = 1;
            var post = new Post
            {
                Id = postId,
                Title = "Test Post",
                Content = "Test Content",
                Thumbnail = "test.jpg",
                Status = "false",
                Type = "Test Type",
                CreatedDate = DateTime.Now
            };

            

            var updatedPost = new Post
            {
                Id = postId,
                Title = "Updated Test Post",
                Content = "Updated Test Content",
                Thumbnail = "test2.jpg",
                Status = "true",
                Type = "Updated Test Type",
                CreatedDate = DateTime.Now
            };

            var fakePostService = A.Fake<IPostService>();
            A.CallTo(() => fakePostService.GetById(postId)).Returns(post);
            var cloudStorageService = A.Fake<ICloudStorageService>();
            var controller = new PostController(fakePostService, cloudStorageService);

            // Act
            var result = await controller.Update(updatedPost);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPost = Assert.IsType<Post>(okResult.Value);

            Assert.Equal(updatedPost.Id, returnedPost.Id);
            Assert.Equal(updatedPost.Title, returnedPost.Title);
            Assert.Equal(updatedPost.Content, returnedPost.Content);
            Assert.Equal(updatedPost.Thumbnail, returnedPost.Thumbnail);
            Assert.Equal(updatedPost.Status, returnedPost.Status);
            Assert.Equal(updatedPost.Type, returnedPost.Type);
            Assert.Equal(updatedPost.CreatedDate, returnedPost.CreatedDate);

            A.CallTo(() => fakePostService.Update(updatedPost)).MustHaveHappenedOnceExactly();
        }
    }
}

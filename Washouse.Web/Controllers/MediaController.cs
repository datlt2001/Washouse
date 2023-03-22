using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Common.Mails;
using Washouse.Data;
using Washouse.Model.ResponseModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/medias")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        public readonly ICloudStorageService _cloudStorageService;
        public MediaController(ICloudStorageService cloudStorageService)
        {
            this._cloudStorageService = cloudStorageService;
        }

        /// <summary>
        /// Upload a image/file.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/medias
        ///     {        
        ///       "Photo": file choosen (multipart/form-data)
        ///     }
        /// </remarks>
        /// <returns>File uploaded information.</returns>
        /// <response code="200">Success return file uploaded information</response>   
        /// <response code="404">Not found any file choosen</response>   
        /// <response code="400">One or more error occurs</response>   
        // POST: api/medias
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile Photo)
        {
            try
            {
                string SavedUrl = null;
                string SignedUrl = null;
                string SavedFileName = null;
                if (Photo != null)
                {
                    SavedFileName = Utilities.GenerateFileNameToSave(Photo.FileName);
                    SavedUrl = await _cloudStorageService.UploadFileAsync(Photo, SavedFileName);
                    SignedUrl = await _cloudStorageService.GetSignedUrlAsync(SavedFileName);
                } else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found file choosen",
                        Data = null
                    });
                }
                return Ok(new ResponseModel
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "success",
                    Data = new ImageResponseModel
                    {
                        SavedUrl = SavedUrl,
                        SavedFileName = SavedFileName,
                        SignedUrl = SignedUrl
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
        /// <summary>
        /// Upload multiple images/files.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST api/medias/upload-multile
        ///     {        
        ///       "Photo": files choosen (multipart/form-data)
        ///     }
        /// </remarks>
        /// <returns>Files uploaded information.</returns>
        /// <response code="200">Success return file uploaded information</response>   
        /// <response code="404">Not found any file choosen</response>   
        /// <response code="400">One or more error occurs</response>   
        // POST: api/medias
        [HttpPost("upload-multile")]
        public async Task<IActionResult> UploadMultile(List<IFormFile> Photos)
        {
            try
            {
                var response = new List<ImageResponseModel>();
                if (Photos.Count > 0)
                {
                    foreach (var photo in Photos)
                    {
                        string SavedUrl = null;
                        string SignedUrl = null;
                        string SavedFileName = null;
                        SavedFileName = Utilities.GenerateFileNameToSave(photo.FileName);
                        SavedUrl = await _cloudStorageService.UploadFileAsync(photo, SavedFileName);
                        SignedUrl = await _cloudStorageService.GetSignedUrlAsync(SavedFileName);
                        response.Add(new ImageResponseModel
                        {
                            SavedUrl = SavedUrl,
                            SavedFileName = SavedFileName,
                            SignedUrl = SignedUrl
                        });

                    }
                    
                }
                else
                {
                    return NotFound(new ResponseModel
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "Not found file choosen",
                        Data = null
                    });
                }
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
    }
}

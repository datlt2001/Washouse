using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography.Xml;
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
        [HttpPost("multiples")]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> Photos)
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

        [HttpPost("link")]
        public async Task<IActionResult> UploadLink(string link)
        {
            try
            {
                //link = "https://lh3.googleusercontent.com/a/AGNmyxZuwoWx82ykgNQKCP-Dnj8I3GLPAFbFW19nKF8P1w=s100";
                string SavedUrl = null;
                string SignedUrl = null;
                string SavedFileName = null;

                SavedFileName = Path.GetFileName(link);

                // Download the file from the link
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(link);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsByteArrayAsync();

                        // Save the file to a temporary location
                        var tempFileName = Path.GetTempFileName();
                        using (var fileStream = new FileStream(tempFileName, FileMode.Create))
                        {
                            await fileStream.WriteAsync(content, 0, content.Length);
                        }

                        // Create an IFormFile from the temporary file
                        var file = new FileInfo(tempFileName);
                        var fileContent = new FileStream(tempFileName, FileMode.Open);
                        var formFile = new FormFile(fileContent, 0, file.Length, file.Name, file.Name)
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "application/octet-stream"
                        };

                        // Upload the file to Google Cloud Storage
                        SavedUrl = await _cloudStorageService.UploadFileAsync(formFile, SavedFileName);
                        SignedUrl = await _cloudStorageService.GetSignedUrlAsync(SavedFileName);

                        // Delete the temporary file
                        fileContent.Dispose();
                        //File.Delete(tempFileName);
                    }
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
    }
}

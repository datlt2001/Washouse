using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Washouse.Common.Helpers;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/serviceCategories")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly IServiceCategoryService _serviceCategoryService;

        public ServiceCategoryController(IServiceCategoryService serviceCategoryService)
        {
            this._serviceCategoryService = serviceCategoryService;
        }

        [HttpGet]
        public IActionResult GetCategoryList()
        {
            var categories = _serviceCategoryService.GetAll();
            if (categories == null) { return NotFound(); }
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var categories = await _serviceCategoryService.GetById(id);
            if (categories == null) { return NotFound(); }
            return Ok(categories);
        }

        [HttpPost("addCategory")]
        public async Task<IActionResult> Create([FromForm]CategoryModel Input)
        {
            if (ModelState.IsValid)
            {
                Input.category.Id = 0;
                Input.category.CreatedDate = DateTime.Now;
                Input.category.UpdatedDate = DateTime.Now;
                Input.category.Status = false;
                //Input.category.Image =  await Utilities.UploadFile(Input.Image, @"images\categories", Input.Image.FileName);
                var categories = _serviceCategoryService.Add(Input.category);
                return Ok(categories);
            }
            else { return BadRequest(); }

        }

        [HttpPut("updateCategory")]
        public async Task<IActionResult> Update([FromForm] CategoryRequestModel category, int id) 
        {
            if(!ModelState.IsValid) { return BadRequest(); }
            else
            {
                Category existingCategorySevice =  await _serviceCategoryService.GetById(id);
                //Category existingCategorySevice = new Category();
                if (existingCategorySevice == null) { return NotFound(); }
                else
                {
                    existingCategorySevice.CategoryName = category.CategoryName;
                    existingCategorySevice.Description = category.Description;
                    existingCategorySevice.UpdatedDate = DateTime.Now;
                    existingCategorySevice.Status = category.Status;
                    existingCategorySevice.HomeFlag= category.HomeFlag;

                    await _serviceCategoryService.Update(existingCategorySevice);
                    return Ok(existingCategorySevice);
                }
                
                
            }
        }

        [HttpPut("deactivateCategory/{id}")]
        public async Task<IActionResult> DeactivateCategory(int id)
        {
            var category = await _serviceCategoryService.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            await _serviceCategoryService.DeactivateCategory(id);
            return Ok();
        }

        [HttpPut("activateCategory/{id}")]
        public async Task<IActionResult> ActivateCategory(int id)
        {
            var category = await _serviceCategoryService.GetById(id);
            if (category == null)
            {
                return NotFound();
            }
            await _serviceCategoryService.ActivateCategory(id);
            return Ok();
        }
    }
}

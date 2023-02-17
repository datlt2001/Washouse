﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Washouse.Data.Infrastructure;
using Washouse.Model.Models;
using Washouse.Service;

namespace Washouse.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {
        private IServiceCategoryService _serviceCategoryService;

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

        [HttpGet("GetParentCategoryList")]
        public IActionResult GetParentCategoryList()
        {
            var categories = _serviceCategoryService.GetAllParentCategory();
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

        [HttpGet("GetCategoryByParentId/{id}")]
        public IActionResult GetCategoryByParentId(int id)
        {
            var categories =  _serviceCategoryService.GetCategoryByParentId(id);
            if (categories == null) { return NotFound(); }
            return Ok(categories);
        }

        [HttpPost("AddCategory")]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.Id = 0;
                category.CreatedDate = DateTime.Now;
                category.UpdatedDate = DateTime.Now;
                category.Status = false;
                var categories = _serviceCategoryService.Add(category);
                return Ok(categories);
            }
            else { return BadRequest(); }

        }

        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> Update(Category category, int id) 
        {
            if(!ModelState.IsValid) { return BadRequest(); }
            else
            {
                //Category existingCategorySevice =  await _serviceCategoryService.GetById(id);
                Category existingCategorySevice = new Category();
                if (existingCategorySevice == null) { return NotFound(); }
                else
                {
                    category.Id = existingCategorySevice.Id;
                    existingCategorySevice = category;

                    await _serviceCategoryService.Update(existingCategorySevice);
                    return Ok(existingCategorySevice);
                }
                
                
            }
        }

        [HttpPut("DeactivateCategory/{id}")]
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

        [HttpPut("ActivateCategory/{id}")]
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

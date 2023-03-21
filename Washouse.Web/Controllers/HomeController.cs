using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Washouse.Data;
using Washouse.Model.Models;
using Washouse.Model.RequestModels;
using Washouse.Service.Interface;
using Washouse.Web.Models;

namespace Washouse.Web.Controllers
{
    [Route("api/homeTest")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly ICloudStorageService _cloudStorageService;
        private readonly WashouseDbContext _context;

        public HomeController(WashouseDbContext context, ICloudStorageService cloudStorageService)
        {
            _context = context;
            _cloudStorageService = cloudStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _context.Categories.ToListAsync();

            foreach (var animal in list)
            {
                var cate = new CategoryRequestModel 
                {
                    CategoryName = animal.CategoryName,
                    Alias = animal.Alias,
                    ParentId = animal.ParentId,
                    Description = animal.Description,
                    Status = animal.Status,
                    HomeFlag = animal.HomeFlag,
                    SavedUrl = animal.Image
                    
                };
                await GenerateSignedUrl(cate);

            }
            return Ok(list);
        }

        [HttpGet("id")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var animal = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animal == null)
            {
                return NotFound();
            }
            var cate = new CategoryRequestModel
            {
                CategoryName = animal.CategoryName,
                Alias = animal.Alias,
                ParentId = animal.ParentId,
                Description = animal.Description,
                Status = animal.Status,
                HomeFlag = animal.HomeFlag,
                SavedUrl = animal.Image
            };
            if (!string.IsNullOrWhiteSpace(animal.Image))
            {
                cate.SignedUrl = await _cloudStorageService.GetSignedUrlAsync(animal.Image);
            }
            return Ok(cate);
        }

        private async Task GenerateSignedUrl(CategoryRequestModel animal)
        {
            // Get Signed URL only when Saved File Name is available.
            if (!string.IsNullOrWhiteSpace(animal.SavedFileName))
            {
                animal.SignedUrl = await _cloudStorageService.GetSignedUrlAsync(animal.SavedFileName);
            }
        }


        // POST: Animals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [HttpPost("uploadImage")]
        public async Task<IActionResult> Create(IFormFile Photo)
        {
            string SavedUrl = null;
            string SignedUrl = null;
            string SavedFileName = null;
            if (Photo != null)
            {
                SavedFileName = GenerateFileNameToSave(Photo.FileName);
                SavedUrl = await _cloudStorageService.UploadFileAsync(Photo, SavedFileName);
            }
            //await _context.Categories.AddAsync(cate);
            //await _context.SaveChangesAsync();
            return Ok(new
            {
                SavedUrl = SavedUrl,
                SavedFileName = SavedFileName,
                SignedUrl = SignedUrl,
                Link = await _cloudStorageService.GetSignedUrlAsync(SavedFileName)
            }); 
        }

        private string? GenerateFileNameToSave(string incomingFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(incomingFileName);
            var extension = Path.GetExtension(incomingFileName);
            return $"{fileName}-{DateTime.Now.ToUniversalTime().ToString("yyyyMMddHHmmss")}{extension}";
        }

        /*// GET: Animals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Animal == null)
            {
                return NotFound();
            }

            var animal = await _context.Animal.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }
            await GenerateSignedUrl(animal);
            return View(animal);
        }

        // POST: Animals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Age,Photo,SavedUrl,SavedFileName")] Animal animal)
        {
            if (id != animal.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await ReplacePhoto(animal);
                    _context.Update(animal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(animal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(animal);
        }

        private async Task ReplacePhoto(Animal animal)
        {
            if (animal.Photo != null)
            {
                //replace the file by deleting animal.SavedFileName file and then uploading new animal.Photo
                if (!string.IsNullOrEmpty(animal.SavedFileName))
                {
                    await _cloudStorageService.DeleteFileAsync(animal.SavedFileName);
                }
                animal.SavedFileName = GenerateFileNameToSave(animal.Photo.FileName);
                animal.SavedUrl = await _cloudStorageService.UploadFileAsync(animal.Photo, animal.SavedFileName);
            }
        }

        // GET: Animals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Animal == null)
            {
                return NotFound();
            }

            var animal = await _context.Animal
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animal == null)
            {
                return NotFound();
            }
            await GenerateSignedUrl(animal);
            return View(animal);
        }

        // POST: Animals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Animal == null)
            {
                return Problem("Entity set 'AnimalKingdomContext.Animal'  is null.");
            }
            var animal = await _context.Animal.FindAsync(id);
            if (animal != null)
            {
                if (!string.IsNullOrWhiteSpace(animal.SavedFileName))
                {
                    await _cloudStorageService.DeleteFileAsync(animal.SavedFileName);
                    animal.SavedFileName = String.Empty;
                    animal.SavedUrl = String.Empty;
                }
                _context.Animal.Remove(animal);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalExists(int id)
        {
            return _context.Animal.Any(e => e.Id == id);
        }*/
    }
}

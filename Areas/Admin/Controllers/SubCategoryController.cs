using DienTu.Data;
using DienTu.Models;
using DienTu.Models.ViewModels;
using DienTu.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DienTu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //get index
        public async Task<IActionResult> Index()
        {

            var subCategories = await _db.SubCategory.Include(a=>a.Category).ToListAsync();
            return View(subCategories);
        }

        //get-create
        public async Task<IActionResult> Create()
        {
            CategoryAndSubCategoryVM model = new CategoryAndSubCategoryVM()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList= await _db.SubCategory.OrderBy(a=>a.Name).Select(a=>a.Name).Distinct().ToListAsync(),
            };
            return View(model);
        }

        //post-create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Create(CategoryAndSubCategoryVM model)
        {
            if(ModelState.IsValid)
            {

                var doesSubCategoryExits = _db.SubCategory.Include(a => a.Category).
                                         Where(a => a.Name == model.SubCategory.Name &&
                                         a.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExits.Count()>0)
                {
                    StatusMessage = "Error : SubCategory have exits in " + doesSubCategoryExits.First().Category.Name + " " +  "pls use another name."; 
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

            }
            CategoryAndSubCategoryVM modelVM = new CategoryAndSubCategoryVM()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(a => a.Name).Select(a => a.Name).ToListAsync(),
                StatusMessage=StatusMessage
            };
            return View(modelVM);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await (from subCategory in _db.SubCategory 
                                   where subCategory.CategoryId == id 
                                   select subCategory).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        //get- edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(a => a.Id == id);
            if(subCategory ==null)
            {
                return NotFound();
            }

            CategoryAndSubCategoryVM model = new CategoryAndSubCategoryVM()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(a=>a.Name).Select(a => a.Name).Distinct().ToListAsync(),
            };
            return View(model);
        }

        //post-edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( CategoryAndSubCategoryVM model)
        {
            if(ModelState.IsValid)
            {
                var doesSubCategoryExit = _db.SubCategory.Include(a => a.Category)
                                          .Where(a => a.Name == model.SubCategory.Name && 
                                          a.Category.Id == model.SubCategory.CategoryId);

                if(doesSubCategoryExit.Count()>0)
                {
                    StatusMessage = "Error: SubCategory exist " + doesSubCategoryExit.First().Category.Name + " " + " pls use another name";
                }
                else
                {
                    var subFromDb = await _db.SubCategory.FindAsync(model.SubCategory.Id);
                    subFromDb.Name = model.SubCategory.Name;

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }
            }
            CategoryAndSubCategoryVM modelVM = new CategoryAndSubCategoryVM()
            {
                SubCategory = model.SubCategory,
                CategoryList = await _db.Category.ToListAsync(),
                SubCategoryList = await _db.SubCategory.OrderBy(a => a.Name).Select(a => a.Name).ToListAsync(),
                StatusMessage = StatusMessage,
            };

            return View(modelVM);
        }

        //get- details
        public async Task<IActionResult> Details(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.Include(a => a.Category).SingleOrDefaultAsync(a => a.Id == id);
            if(subCategory ==null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        // get-delete
        public async Task<IActionResult> Delete(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.Include(a => a.Category).SingleOrDefaultAsync(a => a.Id == id);
            if(subCategory ==null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        //post-delete

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _db.SubCategory.Include(a => a.Category).SingleOrDefaultAsync(a => a.Id == id);
            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

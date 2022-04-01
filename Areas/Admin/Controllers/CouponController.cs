using DienTu.Data;
using DienTu.Models;
using DienTu.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DienTu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.ManagerUser)]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponController( ApplicationDbContext db)
        {
            _db = db;
        }

        //get-index
        public async Task<IActionResult> Index()
        {
            var coupon = await _db.Coupon.ToListAsync();
            return View(coupon);
        }

        //get-create

        public IActionResult Create()
        {
            return View();
        }

        //post-create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if(files.Count>0)
                {
                    byte[] p1 = null;
                    using(var fs1 = files[0].OpenReadStream())
                    {
                        using(var ms1= new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coupon.Picture = p1;
                }

                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        //get-edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.SingleOrDefaultAsync(a => a.Id == id);
            if(coupon==null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        //post-edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if(coupon.Id==0)
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.Where(a => a.Id == coupon.Id).FirstOrDefaultAsync();

            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if(files.Count>0)
                {
                    byte[] p1 = null;
                    using(var fs1 = files[0].OpenReadStream())
                    {
                        using(var ms1= new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDb.Picture = p1;
                }

                couponFromDb.Name = coupon.Name;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.MinimumAmount = coupon.MinimumAmount;
                couponFromDb.IsActive = coupon.IsActive;
                couponFromDb.CouponType = coupon.CouponType;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);

        }

        //get-details
        public async Task<IActionResult> Details(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.FirstOrDefaultAsync(a => a.Id == id);
            if(coupon ==null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        //get-delete
        public async Task<IActionResult> Delete(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.SingleOrDefaultAsync(a => a.Id == id);

            if(coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        //post-delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coupon = await _db.Coupon.SingleOrDefaultAsync(a => a.Id == id);

            _db.Coupon.Remove(coupon);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}

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

    [Authorize(Roles =SD.ManagerUser)]
    [Area("Admin")]
    public class BanHangController : Controller
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public HoaDonVM HoaDonVM { get; set; }

        public BanHangController(ApplicationDbContext db)
        {
            _db = db;
            HoaDonVM = new HoaDonVM
            {
                HoaDon = new Models.HoaDon(),              
            };
        }
        public async Task<IActionResult> Index()
        {
            var hoaDon = await _db.HoaDon.Include(a => a.MenuItem).ToListAsync();
            return View(hoaDon);
        }
        //get-create
        public IActionResult Create()
        {
            HoaDonVM modal = new HoaDonVM();
            return View(modal);
        }


        
        public async Task<IActionResult> ThemSanPham()
        {

            HoaDonVM = new HoaDonVM
            {
                HoaDon = new Models.HoaDon(),
            };
            await _db.SaveChangesAsync();
          return RedirectToAction(nameof(Create));
        }

        public IActionResult LoadDataItem()
        {
            List<MenuItem> model = new List<MenuItem>();
            model = _db.MenuItem.AsQueryable().ToList();
            return PartialView("_Item", model);
        }
    }
}

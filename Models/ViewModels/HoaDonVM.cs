using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DienTu.Models.ViewModels
{
    public class HoaDonVM
    {
        public HoaDon HoaDon { get; set; }

       
        public List<MenuItem> MenuItem { get; set; }

    }
}

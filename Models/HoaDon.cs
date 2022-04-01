using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DienTu.Models
{
    public class HoaDon
    {
        [Key]
        public int MaHD { get; set; }

        public string TenHH { get; set; }
 
        public DateTime NgayDatHang { get; set; }


        public int SoLuong { get; set; }

        public double Vat { get; set; }

        public double ThanhTien { get; set; }

        public double TongCong { get; set; }


        public int MenuItemId { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }
    }
}

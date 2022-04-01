using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DienTu.Data.Migrations
{
    public partial class addHoaDonToDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHD = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKH = table.Column<string>(nullable: true),
                    DiaChi = table.Column<string>(nullable: true),
                    NgayDatHang = table.Column<DateTime>(nullable: false),
                    SDT = table.Column<string>(nullable: true),
                    SoLuong = table.Column<int>(nullable: false),
                    Vat = table.Column<double>(nullable: false),
                    ThanhTien = table.Column<double>(nullable: false),
                    TongCong = table.Column<double>(nullable: false),
                    MenuItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.MaHD);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoaDon");
        }
    }
}

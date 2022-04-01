using Microsoft.EntityFrameworkCore.Migrations;

namespace DienTu.Data.Migrations
{
    public partial class addFixDbHoaDon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "SDT",
                table: "HoaDon");

            migrationBuilder.DropColumn(
                name: "TenKH",
                table: "HoaDon");

            migrationBuilder.AddColumn<string>(
                name: "TenHH",
                table: "HoaDon",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenHH",
                table: "HoaDon");

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SDT",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenKH",
                table: "HoaDon",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

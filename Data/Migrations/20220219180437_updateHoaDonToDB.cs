using Microsoft.EntityFrameworkCore.Migrations;

namespace DienTu.Data.Migrations
{
    public partial class updateHoaDonToDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MenuItemId",
                table: "HoaDon",
                column: "MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoaDon_MenuItem_MenuItemId",
                table: "HoaDon",
                column: "MenuItemId",
                principalTable: "MenuItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoaDon_MenuItem_MenuItemId",
                table: "HoaDon");

            migrationBuilder.DropIndex(
                name: "IX_HoaDon_MenuItemId",
                table: "HoaDon");
        }
    }
}

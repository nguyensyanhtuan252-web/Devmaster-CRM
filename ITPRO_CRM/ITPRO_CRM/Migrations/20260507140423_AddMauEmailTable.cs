using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITPRO_CRM.Migrations
{
    /// <inheritdoc />
    public partial class AddMauEmailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_GiangVien_GiangVienId",
                table: "LopHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_KhoaHoc_KhoaHocId",
                table: "LopHoc");

            migrationBuilder.CreateTable(
                name: "MauEmail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoaiMau = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauEmail", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_GiangVien_GiangVienId",
                table: "LopHoc",
                column: "GiangVienId",
                principalTable: "GiangVien",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_KhoaHoc_KhoaHocId",
                table: "LopHoc",
                column: "KhoaHocId",
                principalTable: "KhoaHoc",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_GiangVien_GiangVienId",
                table: "LopHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_KhoaHoc_KhoaHocId",
                table: "LopHoc");

            migrationBuilder.DropTable(
                name: "MauEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_GiangVien_GiangVienId",
                table: "LopHoc",
                column: "GiangVienId",
                principalTable: "GiangVien",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_KhoaHoc_KhoaHocId",
                table: "LopHoc",
                column: "KhoaHocId",
                principalTable: "KhoaHoc",
                principalColumn: "Id");
        }
    }
}

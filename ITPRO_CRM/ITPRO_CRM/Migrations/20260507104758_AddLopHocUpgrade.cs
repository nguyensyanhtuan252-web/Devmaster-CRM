using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITPRO_CRM.Migrations
{
    /// <inheritdoc />
    public partial class AddLopHocUpgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TenLop",
                table: "LopHoc",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LichHoc",
                table: "LopHoc",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "GiangVienId",
                table: "LopHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GioBatDau",
                table: "LopHoc",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "GioKetThuc",
                table: "LopHoc",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhoaHocId",
                table: "LopHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MoTa",
                table: "LopHoc",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "LopHoc",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PhongHoc",
                table: "LopHoc",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayTao",
                table: "HocVien",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_GiangVienId",
                table: "LopHoc",
                column: "GiangVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_KhoaHocId",
                table: "LopHoc",
                column: "KhoaHocId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_GiangVien_GiangVienId",
                table: "LopHoc");

            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_KhoaHoc_KhoaHocId",
                table: "LopHoc");

            migrationBuilder.DropIndex(
                name: "IX_LopHoc_GiangVienId",
                table: "LopHoc");

            migrationBuilder.DropIndex(
                name: "IX_LopHoc_KhoaHocId",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "GiangVienId",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "GioBatDau",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "GioKetThuc",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "KhoaHocId",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "MoTa",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "PhongHoc",
                table: "LopHoc");

            migrationBuilder.AlterColumn<string>(
                name: "TenLop",
                table: "LopHoc",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LichHoc",
                table: "LopHoc",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayTao",
                table: "HocVien",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}

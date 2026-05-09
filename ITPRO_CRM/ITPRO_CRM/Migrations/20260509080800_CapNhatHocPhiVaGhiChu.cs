using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITPRO_CRM.Migrations
{
    /// <inheritdoc />
    public partial class CapNhatHocPhiVaGhiChu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "EmailGui",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "EmailHeThong",
                table: "CauHinh");

            migrationBuilder.DropColumn(
                name: "MatKhauEmail",
                table: "CauHinh");

            migrationBuilder.AddColumn<string>(
                name: "GhiChuGiaoVien",
                table: "DiemDanh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayGhiChu",
                table: "DiemDanh",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DotThanhToan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    TenDot = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoTienPhaiThu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoTienDaThu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HanThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotThanhToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DotThanhToan_HocVien_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DotThanhToan_HocVienId",
                table: "DotThanhToan",
                column: "HocVienId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DotThanhToan");

            migrationBuilder.DropColumn(
                name: "GhiChuGiaoVien",
                table: "DiemDanh");

            migrationBuilder.DropColumn(
                name: "NgayGhiChu",
                table: "DiemDanh");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailGui",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailHeThong",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatKhauEmail",
                table: "CauHinh",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

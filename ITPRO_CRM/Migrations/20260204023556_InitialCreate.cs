using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITPRO_CRM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CauHinh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTrungTam = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailHeThong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailGui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MatKhauEmail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CauHinh", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChienDich",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChienDich = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiChienDich = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NganSach = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NganSachDuKien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DoanhThuKyVong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DangHoatDong = table.Column<bool>(type: "bit", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChienDich", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiangVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BangCap = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KinhNghiem = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanhTich = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChuyenMon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiangVien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhoaHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKhoaHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HocPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoBuoi = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LopHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLop = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayKhaiGiang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LichHoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HocPhi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SiSoToiDa = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HocVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    FacebookLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZaloLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TruongHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NganhHoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CongViecHienTai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenPhuHuynh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SdtPhuHuynh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrinhDoHienTai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MucTieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguonGoc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NhanVienId = table.Column<int>(type: "int", nullable: true),
                    LopHocId = table.Column<int>(type: "int", nullable: true),
                    ChienDichId = table.Column<int>(type: "int", nullable: true),
                    MucTieuHocTap = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocVien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocVien_ChienDich_ChienDichId",
                        column: x => x.ChienDichId,
                        principalTable: "ChienDich",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HocVien_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HocVien_NhanVien_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "NhanVien",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LopHocId = table.Column<int>(type: "int", nullable: false),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    NgayDiemDanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemDanh_HocVien_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiemDanh_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuTuVan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HinhThuc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KetQua = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayTuVan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    NguoiTuVan = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuTuVan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuTuVan_HocVien_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhieuThu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HocVienId = table.Column<int>(type: "int", nullable: false),
                    LopHocId = table.Column<int>(type: "int", nullable: true),
                    SoTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    NgayThu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiThu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhThuc = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuThu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuThu_HocVien_HocVienId",
                        column: x => x.HocVienId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuThu_LopHoc_LopHocId",
                        column: x => x.LopHocId,
                        principalTable: "LopHoc",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_HocVienId",
                table: "DiemDanh",
                column: "HocVienId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_LopHocId",
                table: "DiemDanh",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_HocVien_ChienDichId",
                table: "HocVien",
                column: "ChienDichId");

            migrationBuilder.CreateIndex(
                name: "IX_HocVien_LopHocId",
                table: "HocVien",
                column: "LopHocId");

            migrationBuilder.CreateIndex(
                name: "IX_HocVien_NhanVienId",
                table: "HocVien",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuTuVan_HocVienId",
                table: "LichSuTuVan",
                column: "HocVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThu_HocVienId",
                table: "PhieuThu",
                column: "HocVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuThu_LopHocId",
                table: "PhieuThu",
                column: "LopHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CauHinh");

            migrationBuilder.DropTable(
                name: "DiemDanh");

            migrationBuilder.DropTable(
                name: "GiangVien");

            migrationBuilder.DropTable(
                name: "KhoaHoc");

            migrationBuilder.DropTable(
                name: "LichSuTuVan");

            migrationBuilder.DropTable(
                name: "PhieuThu");

            migrationBuilder.DropTable(
                name: "HocVien");

            migrationBuilder.DropTable(
                name: "ChienDich");

            migrationBuilder.DropTable(
                name: "LopHoc");

            migrationBuilder.DropTable(
                name: "NhanVien");
        }
    }
}

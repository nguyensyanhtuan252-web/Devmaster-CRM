using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Collections.Generic; // Cần thêm để dùng List
using System.Threading.Tasks;

namespace ITPRO_CRM.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public PublicController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("DangKy")]
        public async Task<IActionResult> DangKy(string cid)
        {
            var cd = await _context.ChienDich.FirstOrDefaultAsync(x => x.MaTracking == cid);
            if (cd != null)
            {
                ViewBag.ChienDichId = cd.Id;
                ViewBag.TenChienDich = cd.TenChienDich;
            }
            else
            {
                ViewBag.TenChienDich = "Tư vấn khóa học ITPRO";
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("DangKy")]
        public async Task<IActionResult> DangKy(HocVien model)
        {
            try
            {
                ModelState.Clear();
                if (string.IsNullOrWhiteSpace(model.HoTen)) ModelState.AddModelError("HoTen", "Họ tên bắt buộc");
                if (string.IsNullOrWhiteSpace(model.SoDienThoai)) ModelState.AddModelError("SoDienThoai", "SĐT bắt buộc");

                if (ModelState.IsValid)
                {
                    model.NgayTao = DateTime.Now;
                    model.TrangThai = 0;
                    if (string.IsNullOrEmpty(model.GioiTinh)) model.GioiTinh = "Chưa rõ";
                    if (model.NgaySinh == default) model.NgaySinh = new DateTime(2000, 1, 1);

                    // 1. Ghi nhận nguồn gốc
                    if (model.ChienDichId.HasValue)
                    {
                        var cd = await _context.ChienDich.FindAsync(model.ChienDichId);
                        if (cd != null) model.NguonGoc = "Chiến dịch: " + cd.TenChienDich;
                    }
                    else
                    {
                        model.NguonGoc = "Đăng ký trực tiếp";
                    }

                    // 2. Chia Sale (Round-Robin)
                    var danhSachSale = await _context.NhanVien.Select(nv => nv.Id).OrderBy(id => id).ToListAsync();
                    if (danhSachSale.Any())
                    {
                        var khachGanNhat = await _context.HocVien.Where(h => h.NhanVienId != null).OrderByDescending(h => h.Id).FirstOrDefaultAsync();
                        if (khachGanNhat != null && khachGanNhat.NhanVienId.HasValue)
                        {
                            int viTriTiepTheo = (danhSachSale.IndexOf(khachGanNhat.NhanVienId.Value) + 1) % danhSachSale.Count;
                            model.NhanVienId = danhSachSale[viTriTiepTheo];
                        }
                        else model.NhanVienId = danhSachSale[0];
                    }

                    _context.Add(model);
                    await _context.SaveChangesAsync();

                    // =========================================================
                    // 3. PHÁT THÔNG BÁO CHO CẢ SALE VÀ ADMIN
                    // =========================================================

                    // Danh sách những người sẽ nhận được thông báo
                    var userIdsToNotify = new List<int>();

                    // Thêm ID của bạn Sale được chia khách
                    if (model.NhanVienId.HasValue)
                    {
                        userIdsToNotify.Add(model.NhanVienId.Value);
                    }

                    // Tìm thêm ID của tất cả các Admin trong hệ thống
                    // (Giả sử cột chức vụ của Tuấn là VaiTro, Admin là 1 hoặc dùng Enum LoaiVaiTro.Admin)
                    var adminIds = await _context.NhanVien
                        .Where(nv => nv.VaiTro ==0) // Tuấn sửa lại số 1 thành giá trị Admin của Tuấn nhé
                        .Select(nv => nv.Id)
                        .ToListAsync();

                    // Gộp danh sách Admin vào danh sách nhận thông báo (tránh trùng lặp nếu Sale đó cũng là Admin)
                    foreach (var adminId in adminIds)
                    {
                        if (!userIdsToNotify.Contains(adminId))
                        {
                            userIdsToNotify.Add(adminId);
                        }
                    }

                    // Vòng lặp tạo thông báo cho mọi người trong danh sách
                    foreach (var userId in userIdsToNotify)
                    {
                        var thongBao = new ThongBao
                        {
                            NhanVienId = userId,
                            TieuDe = "🚀 Khách hàng mới đổ về!",
                            NoiDung = $"Học viên {model.HoTen} vừa đăng ký. Nguồn: {model.NguonGoc}",
                            NgayTao = DateTime.Now,
                            DaDoc = false,
                            LinkUrl = $"/HocViens/Details/{model.Id}"
                        };
                        _context.Add(thongBao);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(CamOn));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi: " + ex.Message);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult CamOn() => View();
    }
}
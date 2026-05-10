using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITPRO_CRM.Controllers
{
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.GiangVien)]
    public class DiemDanhsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public DiemDanhsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // 1. Hiển thị bảng điểm danh
        public async Task<IActionResult> Index(int lopId)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userId = HttpContext.Session.GetInt32("UserId");
            var today = DateTime.Now.Date;

            var lop = await _context.LopHoc
                .Include(l => l.HocViens)
                .FirstOrDefaultAsync(l => l.Id == lopId);

            if (lop == null) return NotFound();

            // 🔐 BẢO MẬT: Chỉ Giảng viên của lớp đó hoặc Admin mới được điểm danh
            // CÁCH SỬA: Map ID từ bảng NhanVien sang bảng GiangVien thông qua Tên hoặc Email
            if (roleId == (int)LoaiVaiTro.GiangVien)
            {
                var currentAcc = await _context.NhanVien.FindAsync(userId);
                if (currentAcc != null)
                {
                    var matchedGV = await _context.GiangVien.FirstOrDefaultAsync(g => g.HoTen == currentAcc.HoTen || g.Email == currentAcc.Email);

                    if (matchedGV == null || lop.GiangVienId != matchedGV.Id)
                    {
                        return RedirectToAction("Index", "LopHocs", new { error = "unauthorized" });
                    }
                }
                else
                {
                    return RedirectToAction("Index", "LopHocs", new { error = "unauthorized" });
                }
            }

            ViewBag.LopHoc = lop;
            ViewBag.Ngay = today;

            var daDiemDanh = await _context.DiemDanh
                .Where(d => d.LopHocId == lopId && d.NgayDiemDanh.Date == today)
                .ToListAsync();

            if (daDiemDanh.Count == 0)
            {
                var listNhap = lop.HocViens?.Select(hv => new DiemDanh
                {
                    LopHocId = lopId,
                    HocVienId = hv.Id,
                    HocVien = hv,
                    TrangThai = 1
                }).ToList() ?? new List<DiemDanh>();
                return View(listNhap);
            }

            foreach (var dd in daDiemDanh)
            {
                dd.HocVien = lop.HocViens?.FirstOrDefault(hv => hv.Id == dd.HocVienId);
            }
            return View(daDiemDanh);
        }

        // 2. Lưu kết quả điểm danh
        [HttpPost]
        public async Task<IActionResult> Save(List<DiemDanh> listDiemDanh, int lopId)
        {
            var roleId = HttpContext.Session.GetInt32("VaiTro");
            var userId = HttpContext.Session.GetInt32("UserId");

            var lop = await _context.LopHoc.FindAsync(lopId);

            // Kiểm tra lại quyền trước khi lưu (áp dụng logic map ID tương tự)
            if (roleId == (int)LoaiVaiTro.GiangVien)
            {
                var currentAcc = await _context.NhanVien.FindAsync(userId);
                if (currentAcc == null) return Forbid();

                var matchedGV = await _context.GiangVien.FirstOrDefaultAsync(g => g.HoTen == currentAcc.HoTen || g.Email == currentAcc.Email);

                if (lop == null || matchedGV == null || lop.GiangVienId != matchedGV.Id)
                {
                    return Forbid();
                }
            }

            var today = DateTime.Now.Date;
            var oldData = await _context.DiemDanh
                .Where(d => d.LopHocId == lopId && d.NgayDiemDanh.Date == today)
                .ToListAsync();

            _context.DiemDanh.RemoveRange(oldData);

            foreach (var item in listDiemDanh)
            {
                item.Id = 0;
                item.NgayDiemDanh = today;
                _context.DiemDanh.Add(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "LopHocs", new { id = lopId });
        }
    }
}
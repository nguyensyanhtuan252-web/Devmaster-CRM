using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;
using ITPRO_CRM.Filters;
using Microsoft.AspNetCore.Http; // Bắt buộc phải có để dùng Session

namespace ITPRO_CRM.Controllers
{
    // CẤP QUYỀN TRUY CẬP CHUNG: Cả Admin và Sale đều được vào xem danh sách mẫu
    [PhanQuyen(LoaiVaiTro.Admin, LoaiVaiTro.Sale)]
    public class MauEmailsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public MauEmailsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. XEM DANH SÁCH (AI CŨNG XEM ĐƯỢC)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            return View(await _context.MauEmail.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var mauEmail = await _context.MauEmail.FirstOrDefaultAsync(m => m.Id == id);
            if (mauEmail == null) return NotFound();
            return View(mauEmail);
        }

        // ==========================================
        // 2. TẠO MỚI (CHỈ ADMIN) - 🔐 Đã chặn bằng Session
        // ==========================================
        public IActionResult Create()
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) // 0 là Quyền Admin
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenMau,TieuDe,NoiDung,MoTa")] MauEmail mauEmail)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                _context.Add(mauEmail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mauEmail);
        }

        // ==========================================
        // 3. CHỈNH SỬA (CHỈ ADMIN) - 🔐 Đã chặn bằng Session
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0)
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();
            var mauEmail = await _context.MauEmail.FindAsync(id);
            if (mauEmail == null) return NotFound();
            return View(mauEmail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenMau,TieuDe,NoiDung,MoTa")] MauEmail mauEmail)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");

            if (id != mauEmail.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mauEmail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MauEmailExists(mauEmail.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mauEmail);
        }

        // ==========================================
        // 4. XÓA (CHỈ ADMIN) - 🔐 Đã chặn bằng Session
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0)
                return RedirectToAction("Index", "Home");

            if (id == null) return NotFound();
            var mauEmail = await _context.MauEmail.FirstOrDefaultAsync(m => m.Id == id);
            if (mauEmail == null) return NotFound();
            return View(mauEmail);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("VaiTro") != 0) return RedirectToAction("Index", "Home");

            var mauEmail = await _context.MauEmail.FindAsync(id);
            if (mauEmail != null) _context.MauEmail.Remove(mauEmail);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MauEmailExists(int id) => _context.MauEmail.Any(e => e.Id == id);
    }
}
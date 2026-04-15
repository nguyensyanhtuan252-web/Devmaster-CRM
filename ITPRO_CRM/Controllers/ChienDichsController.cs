using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ITPRO_CRM.Data;
using ITPRO_CRM.Models;

namespace ITPRO_CRM.Controllers
{
    public class ChienDichsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public ChienDichsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: ChienDichs
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChienDich.ToListAsync());
        }

        // GET: ChienDichs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chienDich = await _context.ChienDich
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chienDich == null)
            {
                return NotFound();
            }

            return View(chienDich);
        }

        // GET: ChienDichs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChienDichs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenChienDich,LoaiChienDich,NgayBatDau,NgayKetThuc,NganSach,DoanhThuKyVong,DangHoatDong,MoTa")] ChienDich chienDich)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chienDich);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chienDich);
        }

        // GET: ChienDichs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chienDich = await _context.ChienDich.FindAsync(id);
            if (chienDich == null)
            {
                return NotFound();
            }
            return View(chienDich);
        }

        // POST: ChienDichs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenChienDich,LoaiChienDich,NgayBatDau,NgayKetThuc,NganSach,DoanhThuKyVong,DangHoatDong,MoTa")] ChienDich chienDich)
        {
            if (id != chienDich.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chienDich);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChienDichExists(chienDich.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chienDich);
        }

        // GET: ChienDichs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chienDich = await _context.ChienDich
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chienDich == null)
            {
                return NotFound();
            }

            return View(chienDich);
        }

        // POST: ChienDichs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chienDich = await _context.ChienDich.FindAsync(id);
            if (chienDich != null)
            {
                _context.ChienDich.Remove(chienDich);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChienDichExists(int id)
        {
            return _context.ChienDich.Any(e => e.Id == id);
        }
        // GET: ChienDichs/BaoCao
        public async Task<IActionResult> BaoCao()
        {
            // 1. Lấy dữ liệu Chiến dịch kèm theo Học viên và Lớp học (để tính tiền)
            var rawData = await _context.ChienDich
                .Include(c => c.HocViens)
                .ThenInclude(h => h.LopHoc) // Kèm lớp để lấy Học phí
                .ToListAsync();

            // 2. Chế biến dữ liệu để vẽ biểu đồ
            var reportData = rawData.Select(x => new
            {
                Ten = x.TenChienDich,
                ChiPhi = x.NganSach, // Tiền chi ra (Ngân sách)

                // Tiền thu về = Tổng học phí của tất cả học viên thuộc chiến dịch này
                // (Chỉ tính những người đã xếp lớp, tức là LopHoc != null)
                DoanhThu = x.HocViens.Sum(h => h.LopHoc?.HocPhi ?? 0),

                SoKhach = x.HocViens.Count
            }).ToList();

            // 3. Đóng gói gửi sang View
            ViewBag.ChartLabel = Newtonsoft.Json.JsonConvert.SerializeObject(reportData.Select(x => x.Ten));
            ViewBag.ChartChiPhi = Newtonsoft.Json.JsonConvert.SerializeObject(reportData.Select(x => x.ChiPhi));
            ViewBag.ChartDoanhThu = Newtonsoft.Json.JsonConvert.SerializeObject(reportData.Select(x => x.DoanhThu));

            return View();
        }
    }
}

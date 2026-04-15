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
    public class LichSuTuVansController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public LichSuTuVansController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: LichSuTuVans
        public async Task<IActionResult> Index()
        {
            var iTPRO_CRMContext = _context.LichSuTuVan.Include(l => l.HocVien);
            return View(await iTPRO_CRMContext.ToListAsync());
        }

        // GET: LichSuTuVans/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichSuTuVan = await _context.LichSuTuVan
                .Include(l => l.HocVien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichSuTuVan == null)
            {
                return NotFound();
            }

            return View(lichSuTuVan);
        }

        // GET: LichSuTuVans/Create
        public IActionResult Create()
        {
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen");
            return View();
        }

        // POST: LichSuTuVans/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NoiDung,KetQua,NgayGio,HocVienId")] LichSuTuVan lichSuTuVan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lichSuTuVan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", lichSuTuVan.HocVienId);
            return View(lichSuTuVan);
        }

        // GET: LichSuTuVans/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichSuTuVan = await _context.LichSuTuVan.FindAsync(id);
            if (lichSuTuVan == null)
            {
                return NotFound();
            }
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", lichSuTuVan.HocVienId);
            return View(lichSuTuVan);
        }

        // POST: LichSuTuVans/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NoiDung,KetQua,NgayGio,HocVienId")] LichSuTuVan lichSuTuVan)
        {
            if (id != lichSuTuVan.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lichSuTuVan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LichSuTuVanExists(lichSuTuVan.Id))
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
            ViewData["HocVienId"] = new SelectList(_context.HocVien, "Id", "HoTen", lichSuTuVan.HocVienId);
            return View(lichSuTuVan);
        }

        // GET: LichSuTuVans/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichSuTuVan = await _context.LichSuTuVan
                .Include(l => l.HocVien)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lichSuTuVan == null)
            {
                return NotFound();
            }

            return View(lichSuTuVan);
        }

        // POST: LichSuTuVans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lichSuTuVan = await _context.LichSuTuVan.FindAsync(id);
            if (lichSuTuVan != null)
            {
                _context.LichSuTuVan.Remove(lichSuTuVan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LichSuTuVanExists(int id)
        {
            return _context.LichSuTuVan.Any(e => e.Id == id);
        }
    }
}

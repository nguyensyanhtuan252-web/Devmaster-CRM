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
    public class MauEmailsController : Controller
    {
        private readonly ITPRO_CRMContext _context;

        public MauEmailsController(ITPRO_CRMContext context)
        {
            _context = context;
        }

        // GET: MauEmails
        public async Task<IActionResult> Index()
        {
            return View(await _context.MauEmail.ToListAsync());
        }

        // GET: MauEmails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mauEmail = await _context.MauEmail
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mauEmail == null)
            {
                return NotFound();
            }

            return View(mauEmail);
        }

        // GET: MauEmails/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MauEmails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenMau,TieuDe,NoiDung,NgayTao,LoaiMau")] MauEmail mauEmail)
        {
            if (ModelState.IsValid)
            {
                mauEmail.NgayTao = DateTime.Now;
                _context.Add(mauEmail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mauEmail);
        }

        // GET: MauEmails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mauEmail = await _context.MauEmail.FindAsync(id);
            if (mauEmail == null)
            {
                return NotFound();
            }
            return View(mauEmail);
        }

        // POST: MauEmails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenMau,TieuDe,NoiDung,NgayTao,LoaiMau")] MauEmail mauEmail)
        {
            if (id != mauEmail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mauEmail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MauEmailExists(mauEmail.Id))
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
            return View(mauEmail);
        }

        // GET: MauEmails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mauEmail = await _context.MauEmail
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mauEmail == null)
            {
                return NotFound();
            }

            return View(mauEmail);
        }

        // POST: MauEmails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mauEmail = await _context.MauEmail.FindAsync(id);
            if (mauEmail != null)
            {
                _context.MauEmail.Remove(mauEmail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MauEmailExists(int id)
        {
            return _context.MauEmail.Any(e => e.Id == id);
        }
    }
}

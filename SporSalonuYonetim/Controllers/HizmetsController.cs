using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using Microsoft.AspNetCore.Authorization;

namespace SporSalonuYonetim.Controllers
{
    // KURAL 1: Sayfaları görmek için giriş yapmış olmak şart
    [Authorize]
    public class HizmetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HizmetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hizmets (Herkes görebilir)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Hizmetler.ToListAsync());
        }

        // GET: Hizmets/Details/5 (Herkes görebilir)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .FirstOrDefaultAsync(m => m.HizmetId == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // -----------------------------------------------------------
        // YÖNETİM İŞLEMLERİ (SADECE ADMIN)
        // -----------------------------------------------------------

        // GET: Hizmets/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Hizmets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("HizmetId,Ad,Aciklama,Ucret,SureDakika")] Hizmet hizmet)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hizmet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hizmet);
        }

        // GET: Hizmets/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet == null) return NotFound();
            return View(hizmet);
        }

        // POST: Hizmets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("HizmetId,Ad,Aciklama,Ucret,SureDakika")] Hizmet hizmet)
        {
            if (id != hizmet.HizmetId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hizmet);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HizmetExists(hizmet.HizmetId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hizmet);
        }

        // GET: Hizmets/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hizmet = await _context.Hizmetler
                .FirstOrDefaultAsync(m => m.HizmetId == id);

            if (hizmet == null) return NotFound();

            return View(hizmet);
        }

        // POST: Hizmets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hizmet = await _context.Hizmetler.FindAsync(id);
            if (hizmet != null)
            {
                _context.Hizmetler.Remove(hizmet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HizmetExists(int id)
        {
            return _context.Hizmetler.Any(e => e.HizmetId == id);
        }
    }
}
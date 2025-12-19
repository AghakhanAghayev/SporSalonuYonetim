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
    public class AntrenorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Antrenors
        public async Task<IActionResult> Index()
        {
            var antrenorler = _context.Antrenorler.Include(a => a.SporSalonu);
            return View(await antrenorler.ToListAsync());
        }

        // GET: Antrenors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(m => m.AntrenorId == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // GET: Antrenors/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var hizmetIsimleri = _context.Hizmetler.Select(h => h.Ad).Distinct().ToList();
            if (hizmetIsimleri.Count == 0) hizmetIsimleri.Add("Lütfen Önce Hizmet Ekleyiniz");
            ViewBag.Uzmanliklar = hizmetIsimleri;
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad");

            return View();
        }

        // POST: Antrenors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // DÜZELTME: "Cinsiyet" eklendi
        public async Task<IActionResult> Create([Bind("AntrenorId,AdSoyad,Cinsiyet,UzmanlikAlani,CalismaBaslangic,CalismaBitis,SporSalonuId,ResimUrl")] Antrenor antrenor)
        {
            ModelState.Remove("SporSalonu");

            // Antrenör çalışma saatlerinin salon saatleri dahilinde olup olmadığını kontrol et
            var salon = await _context.SporSalonlari.FindAsync(antrenor.SporSalonuId);
            if (salon != null)
            {
                if (antrenor.CalismaBaslangic < salon.AcilisSaati || antrenor.CalismaBitis > salon.KapanisSaati)
                {
                    ModelState.AddModelError("CalismaBaslangic", $"Antrenör çalışma saatleri salon saatleri dahilinde olmalıdır. Salon saatleri: {salon.AcilisSaati:hh\\:mm} - {salon.KapanisSaati:hh\\:mm}");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var hizmetIsimleri = _context.Hizmetler.Select(h => h.Ad).Distinct().ToList();
            ViewBag.Uzmanliklar = hizmetIsimleri;
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", antrenor.SporSalonuId);
            return View(antrenor);
        }

        // GET: Antrenors/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewBag.Uzmanliklar = _context.Hizmetler.Select(h => h.Ad).Distinct().ToList();
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", antrenor.SporSalonuId);
            return View(antrenor);
        }

        // POST: Antrenors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        // DÜZELTME: "Cinsiyet" eklendi
        public async Task<IActionResult> Edit(int id, [Bind("AntrenorId,AdSoyad,Cinsiyet,UzmanlikAlani,CalismaBaslangic,CalismaBitis,SporSalonuId,ResimUrl")] Antrenor antrenor)
        {
            if (id != antrenor.AntrenorId) return NotFound();

            ModelState.Remove("SporSalonu");

            // Antrenör çalışma saatlerinin salon saatleri dahilinde olup olmadığını kontrol et
            var salon = await _context.SporSalonlari.FindAsync(antrenor.SporSalonuId);
            if (salon != null)
            {
                if (antrenor.CalismaBaslangic < salon.AcilisSaati || antrenor.CalismaBitis > salon.KapanisSaati)
                {
                    ModelState.AddModelError("CalismaBaslangic", $"Antrenör çalışma saatleri salon saatleri dahilinde olmalıdır. Salon saatleri: {salon.AcilisSaati:hh\\:mm} - {salon.KapanisSaati:hh\\:mm}");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.AntrenorId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Uzmanliklar = _context.Hizmetler.Select(h => h.Ad).Distinct().ToList();
            ViewData["SporSalonuId"] = new SelectList(_context.SporSalonlari, "Id", "Ad", antrenor.SporSalonuId);
            return View(antrenor);
        }

        // GET: Antrenors/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(m => m.AntrenorId == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: Antrenors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null) _context.Antrenorler.Remove(antrenor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorId == id);
        }
    }
}
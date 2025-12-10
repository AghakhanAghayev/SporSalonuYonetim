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
    // KURAL 1: İçeri girmek için Üye veya Admin olmak yeterli (Admin zorunluluğunu buradan kaldırdık)
    [Authorize]
    public class AntrenorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // OKUMA İŞLEMLERİ (HERKES GÖREBİLİR)
        // ---------------------------------------------------------------

        // GET: Antrenors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Antrenorler.ToListAsync());
        }

        // GET: Antrenors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .FirstOrDefaultAsync(m => m.AntrenorId == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // ---------------------------------------------------------------
        // YÖNETİM İŞLEMLERİ (SADECE ADMIN YAPABİLİR)
        // ---------------------------------------------------------------

        // GET: Antrenors/Create
        [Authorize(Roles = "Admin")] // KİLİT: Sadece Admin
        public IActionResult Create()
        {
            // Hizmet listesini veritabanından çekiyoruz
            var hizmetIsimleri = _context.Hizmetler
                                         .Select(h => h.Ad)
                                         .Distinct()
                                         .ToList();

            if (hizmetIsimleri.Count == 0)
            {
                hizmetIsimleri.Add("Lütfen Önce Hizmet Ekleyiniz");
            }

            ViewBag.Uzmanliklar = hizmetIsimleri;
            return View();
        }

        // POST: Antrenors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KİLİT: Sadece Admin
        public async Task<IActionResult> Create([Bind("AntrenorId,AdSoyad,UzmanlikAlani,CalismaBaslangic,CalismaBitis")] Antrenor antrenor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(antrenor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Uzmanliklar = _context.Hizmetler.Select(h => h.Ad).Distinct().ToList();
            return View(antrenor);
        }

        // GET: Antrenors/Edit/5
        [Authorize(Roles = "Admin")] // KİLİT: Sadece Admin
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewBag.Uzmanliklar = _context.Hizmetler.Select(h => h.Ad).Distinct().ToList();

            return View(antrenor);
        }

        // POST: Antrenors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KİLİT: Sadece Admin
        public async Task<IActionResult> Edit(int id, [Bind("AntrenorId,AdSoyad,UzmanlikAlani,CalismaBaslangic,CalismaBitis")] Antrenor antrenor)
        {
            if (id != antrenor.AntrenorId) return NotFound();

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
            return View(antrenor);
        }

        // GET: Antrenors/Delete/5
        [Authorize(Roles = "Admin")] // KİLİT: Sadece Admin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .FirstOrDefaultAsync(m => m.AntrenorId == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: Antrenors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // KİLİT: Sadece Admin
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.AntrenorId == id);
        }
    }
}
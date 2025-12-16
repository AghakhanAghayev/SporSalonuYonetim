using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Controllers
{
    public class SporSalonusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SporSalonusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SporSalonus
        public async Task<IActionResult> Index()
        {
            return View(await _context.SporSalonlari.ToListAsync());
        }

        // GET: SporSalonus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.SporSalonlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // GET: SporSalonus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: SporSalonus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GÜNCELLEME: Harita için "Enlem" ve "Boylam" alanları eklendi
        public async Task<IActionResult> Create([Bind("Id,Ad,Sehir,Adres,Telefon,Kapasite,ResimUrl,AcilisSaati,KapanisSaati,Enlem,Boylam")] SporSalonu sporSalonu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sporSalonu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sporSalonu);
        }

        // GET: SporSalonus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.SporSalonlari.FindAsync(id);
            if (sporSalonu == null) return NotFound();
            return View(sporSalonu);
        }

        // POST: SporSalonus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // GÜNCELLEME: Harita için "Enlem" ve "Boylam" alanları eklendi
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Sehir,Adres,Telefon,Kapasite,ResimUrl,AcilisSaati,KapanisSaati,Enlem,Boylam")] SporSalonu sporSalonu)
        {
            if (id != sporSalonu.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sporSalonu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SporSalonuExists(sporSalonu.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sporSalonu);
        }

        // GET: SporSalonus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sporSalonu = await _context.SporSalonlari
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sporSalonu == null) return NotFound();

            return View(sporSalonu);
        }

        // POST: SporSalonus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sporSalonu = await _context.SporSalonlari.FindAsync(id);
            if (sporSalonu != null)
            {
                _context.SporSalonlari.Remove(sporSalonu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SporSalonuExists(int id)
        {
            return _context.SporSalonlari.Any(e => e.Id == id);
        }
    }
}
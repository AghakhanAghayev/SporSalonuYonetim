using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SporSalonuYonetim.Controllers
{
    [Authorize]
    public class RandevusController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Randevus
        public async Task<IActionResult> Index()
        {
            var randevular = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye);
            return View(await randevular.ToListAsync());
        }

        // GET: Randevus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .FirstOrDefaultAsync(m => m.RandevuId == id);

            if (randevu == null) return NotFound();

            return View(randevu);
        }

        // -------------------------------------------------------------------
        // CREATE (RANDEVU ALMA) - MANTIK KONTROLLERİ BURADA
        // -------------------------------------------------------------------

        // GET: Randevus/Create
        public IActionResult Create()
        {
            var antrenorListesi = _context.Antrenorler
                .Select(a => new
                {
                    a.AntrenorId,
                    AdVeUzmanlik = a.AdSoyad + " (" + a.UzmanlikAlani + ")"
                })
                .ToList();

            ViewData["AntrenorId"] = new SelectList(antrenorListesi, "AntrenorId", "AdVeUzmanlik");
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad");

            if (User.IsInRole("Admin"))
            {
                ViewData["UyeId"] = new SelectList(_context.Users, "Id", "UserName");
            }

            return View();
        }

        // POST: Randevus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RandevuId,TarihSaat,Durum,UyeId,AntrenorId,HizmetId")] Randevu randevu)
        {
            // 1. ÜYE ATAMASI
            if (!User.IsInRole("Admin") || string.IsNullOrEmpty(randevu.UyeId))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                randevu.UyeId = userId;
            }

            // 2. OTOMATİK DURUM
            randevu.Durum = "Bekliyor";

            // 3. MODEL TEMİZLİĞİ
            ModelState.Remove("UyeId");
            ModelState.Remove("Uye");
            ModelState.Remove("Durum");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            // ========================================================================
            // 🕵️‍♂️ KRİTİK KONTROLLER (MESAİ VE ÇAKIŞMA)
            // ========================================================================

            // A) Seçilen Antrenör ve Hizmet bilgilerini veritabanından çekelim
            var secilenAntrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);
            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);

            if (secilenAntrenor != null && secilenHizmet != null)
            {
                // Randevunun biteceği saati hesapla (Başlangıç + Hizmet Süresi)
                DateTime baslangicZamani = randevu.TarihSaat;
                DateTime bitisZamani = baslangicZamani.AddMinutes(secilenHizmet.SureDakika);

                // --- KONTROL 1: ANTRENÖR MESAİ SAATLERİ ---
                // Sadece saat kısmını (TimeSpan) karşılaştırıyoruz.
                TimeSpan randevuBaslangicSaat = baslangicZamani.TimeOfDay;
                TimeSpan randevuBitisSaat = bitisZamani.TimeOfDay;

                if (randevuBaslangicSaat < secilenAntrenor.CalismaBaslangic ||
                    randevuBitisSaat > secilenAntrenor.CalismaBitis)
                {
                    ModelState.AddModelError("TarihSaat",
                        $"Antrenör bu saatlerde çalışmıyor. Mesai saatleri: {secilenAntrenor.CalismaBaslangic} - {secilenAntrenor.CalismaBitis}");
                }

                // --- KONTROL 2: RANDEVU ÇAKIŞMASI (OVERLAP) ---
                // Aynı antrenörün, aynı günündeki diğer randevularına bak.
                var cakismanRandevular = await _context.Randevular
                    .Include(r => r.Hizmet) // Hizmet süresini bilmemiz lazım
                    .Where(r => r.AntrenorId == randevu.AntrenorId && r.TarihSaat.Date == randevu.TarihSaat.Date)
                    .ToListAsync();

                foreach (var mevcutRandevu in cakismanRandevular)
                {
                    DateTime mevcutBaslangic = mevcutRandevu.TarihSaat;
                    DateTime mevcutBitis = mevcutBaslangic.AddMinutes(mevcutRandevu.Hizmet.SureDakika);

                    // Çakışma Mantığı: (YeniBaşlangıç < MevcutBitiş) VE (YeniBitiş > MevcutBaşlangıç)
                    if (baslangicZamani < mevcutBitis && bitisZamani > mevcutBaslangic)
                    {
                        ModelState.AddModelError("TarihSaat",
                            $"Seçilen saatte antrenör dolu! ({mevcutBaslangic.ToShortTimeString()} - {mevcutBitis.ToShortTimeString()} arası dolu)");
                        break; // İlk çakışmada hatayı ver ve döngüden çık
                    }
                }
            }
            // ========================================================================

            if (ModelState.IsValid)
            {
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // HATA VARSA SAYFAYI TEKRAR DOLDUR
            var antrenorListesi = _context.Antrenorler
                .Select(a => new { a.AntrenorId, AdVeUzmanlik = a.AdSoyad + " (" + a.UzmanlikAlani + ")" })
                .ToList();

            ViewData["AntrenorId"] = new SelectList(antrenorListesi, "AntrenorId", "AdVeUzmanlik", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);

            if (User.IsInRole("Admin"))
            {
                ViewData["UyeId"] = new SelectList(_context.Users, "Id", "UserName", randevu.UyeId);
            }

            return View(randevu);
        }

        // -------------------------------------------------------------------
        // EDIT VE DELETE İŞLEMLERİ (SADECE ADMIN)
        // -------------------------------------------------------------------

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "AntrenorId", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);
            return View(randevu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RandevuId,TarihSaat,Durum,UyeId,AntrenorId,HizmetId")] Randevu randevu)
        {
            if (id != randevu.RandevuId) return NotFound();
            ModelState.Remove("Uye");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(randevu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RandevuExists(randevu.RandevuId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AntrenorId"] = new SelectList(_context.Antrenorler, "AntrenorId", "AdSoyad", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);
            return View(randevu);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .FirstOrDefaultAsync(m => m.RandevuId == id);
            if (randevu == null) return NotFound();
            return View(randevu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null) _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RandevuExists(int id)
        {
            return _context.Randevular.Any(e => e.RandevuId == id);
        }
    }
}
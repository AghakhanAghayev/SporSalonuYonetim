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

        // -------------------------------------------------------------------
        // INDEX (LİSTELEME) - FİLTRELEME VE OTOMATİK TAMAMLANDI KONTROLÜ
        // -------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            // 1. Sorguyu Hazırla (Henüz veritabanından çekmiyoruz)
            var randevularQuery = _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet) // Hizmet süresi için gerekli
                .Include(r => r.Uye)
                .AsQueryable(); // Üzerine 'Where' şartı ekleyebilmek için bunu ekledik

            // 2. KONTROL: Eğer kullanıcı ADMIN DEĞİLSE, sadece kendi randevularını görsün.
            // Admin ise 'Where' eklenmediği için herkesi görür.
            if (!User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                randevularQuery = randevularQuery.Where(r => r.UyeId == currentUserId);
            }

            // 3. Veriyi şimdi çekiyoruz
            var randevular = await randevularQuery.ToListAsync();

            // 4. OTOMATİK DURUM GÜNCELLEME (Tamamlandı Kontrolü)
            bool degisiklikVarMi = false;
            foreach (var item in randevular)
            {
                // Eğer randevu aktifse ve süresi dolmuşsa
                if (item.Durum != "İptal" && item.Durum != "Reddedildi" && item.Durum != "Tamamlandı")
                {
                    DateTime bitisZamani = item.TarihSaat.AddMinutes(item.Hizmet.SureDakika);

                    if (DateTime.Now > bitisZamani)
                    {
                        item.Durum = "Tamamlandı";
                        _context.Update(item);
                        degisiklikVarMi = true;
                    }
                }
            }

            if (degisiklikVarMi)
            {
                await _context.SaveChangesAsync();
            }

            return View(randevular);
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

            // GÜVENLİK KONTROLÜ: Başkasının detayına bakamasın
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && randevu.UyeId != currentUserId)
            {
                return Unauthorized(); // Yetkisiz Erişim
            }

            return View(randevu);
        }

        // -------------------------------------------------------------------
        // RANDEVU İPTAL ETME (ÜYE VE ADMİN İÇİN)
        // -------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            // Güvenlik: Başkasının randevusunu iptal etmeye çalışıyorsa engelle (Admin hariç)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && randevu.UyeId != userId)
            {
                return Unauthorized();
            }

            // Kural: Geçmiş randevu iptal edilemez
            if (randevu.TarihSaat < DateTime.Now)
            {
                TempData["Hata"] = "Geçmiş randevular iptal edilemez.";
                return RedirectToAction(nameof(Index));
            }

            randevu.Durum = "İptal";
            _context.Update(randevu);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // -------------------------------------------------------------------
        // CREATE (RANDEVU ALMA)
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
            if (!User.IsInRole("Admin") || string.IsNullOrEmpty(randevu.UyeId))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                randevu.UyeId = userId;
            }

            randevu.Durum = "Bekliyor";

            ModelState.Remove("UyeId");
            ModelState.Remove("Uye");
            ModelState.Remove("Durum");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            // --- KONTROLLER BAŞLIYOR ---

            // 1. GEÇMİŞ TARİH KONTROLÜ
            if (randevu.TarihSaat < DateTime.Now)
            {
                ModelState.AddModelError("TarihSaat", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            var secilenAntrenor = await _context.Antrenorler.FindAsync(randevu.AntrenorId);
            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);

            if (secilenAntrenor != null && secilenHizmet != null)
            {
                DateTime baslangicZamani = randevu.TarihSaat;
                DateTime bitisZamani = baslangicZamani.AddMinutes(secilenHizmet.SureDakika);

                // 2. MESAİ SAATİ KONTROLÜ
                TimeSpan randevuBaslangicSaat = baslangicZamani.TimeOfDay;
                TimeSpan randevuBitisSaat = bitisZamani.TimeOfDay;

                if (randevuBaslangicSaat < secilenAntrenor.CalismaBaslangic ||
                    randevuBitisSaat > secilenAntrenor.CalismaBitis)
                {
                    ModelState.AddModelError("TarihSaat",
                        $"Antrenör bu saatlerde çalışmıyor. Mesai: {secilenAntrenor.CalismaBaslangic} - {secilenAntrenor.CalismaBitis}");
                }

                // 3. ÇAKIŞMA KONTROLÜ (İptal edilenler hariç)
                var cakismanRandevular = await _context.Randevular
                    .Include(r => r.Hizmet)
                    .Where(r => r.AntrenorId == randevu.AntrenorId
                             && r.TarihSaat.Date == randevu.TarihSaat.Date
                             && r.Durum != "İptal"
                             && r.Durum != "Reddedildi")
                    .ToListAsync();

                foreach (var mevcutRandevu in cakismanRandevular)
                {
                    DateTime mevcutBaslangic = mevcutRandevu.TarihSaat;
                    DateTime mevcutBitis = mevcutBaslangic.AddMinutes(mevcutRandevu.Hizmet.SureDakika);

                    if (baslangicZamani < mevcutBitis && bitisZamani > mevcutBaslangic)
                    {
                        ModelState.AddModelError("TarihSaat",
                            $"Seçilen saatte antrenör dolu! ({mevcutBaslangic.ToShortTimeString()} - {mevcutBitis.ToShortTimeString()} arası dolu)");
                        break;
                    }
                }
            }
            // --- KONTROLLER BİTTİ ---

            if (ModelState.IsValid)
            {
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

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
        // EDIT VE DELETE (ADMİN)
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
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
        // INDEX (LİSTELEME + OTOMATİK TAMAMLANDI YAPMA)
        // -------------------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var randevularQuery = _context.Randevular
                .Include(r => r.Antrenor)
                    .ThenInclude(a => a.SporSalonu) // <-- İŞTE BU EKSİKTİ! ARTIK ŞUBE GELECEK.
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                randevularQuery = randevularQuery.Where(r => r.UyeId == currentUserId);
            }

            var randevular = await randevularQuery.ToListAsync();

            // --- 1. OTOMATİK "TAMAMLANDI" KONTROLÜ ---
            // Burası senin istediğin gibi çalışıyor: Randevu Saati + Hizmet Süresi dolduysa tamamlar.
            bool degisiklikVarMi = false;
            foreach (var item in randevular)
            {
                if (item.Durum != "İptal" && item.Durum != "Reddedildi" && item.Durum != "Tamamlandı")
                {
                    // Örn: Randevu 14:00, Süre 60dk => Bitiş 15:00
                    DateTime bitisZamani = item.TarihSaat.AddMinutes(item.Hizmet.SureDakika);

                    // Saat 15:01 olduysa Tamamlandı yap
                    if (DateTime.Now > bitisZamani)
                    {
                        item.Durum = "Tamamlandı";
                        _context.Update(item);
                        degisiklikVarMi = true;
                    }
                }
            }

            if (degisiklikVarMi) await _context.SaveChangesAsync();
            // ---------------------------------------------------------

            return View(randevular);
        }

        // GET: Randevus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var randevu = await _context.Randevular
                .Include(r => r.Antrenor)
                    .ThenInclude(a => a.SporSalonu) // Detayda da şube görünsün diye buraya da ekledim
                .Include(r => r.Hizmet)
                .Include(r => r.Uye)
                .FirstOrDefaultAsync(m => m.RandevuId == id);

            if (randevu == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && randevu.UyeId != currentUserId)
            {
                return Unauthorized();
            }

            return View(randevu);
        }

        // -------------------------------------------------------------------
        // CREATE (RANDEVU ALMA + ÇAKIŞMA KONTROLLERİ)
        // -------------------------------------------------------------------

        // GET: Randevus/Create
        public IActionResult Create()
        {
            var antrenorListesi = _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Select(a => new
                {
                    a.AntrenorId,
                    // Örnek: Ahmet Yılmaz (Fitness) - Merkez Şube
                    AdVeUzmanlik = a.AdSoyad + " (" + a.UzmanlikAlani + ") - " + (a.SporSalonu != null ? a.SporSalonu.Ad : "Salon Yok")
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

            // --- 3. KONTROLLER BAŞLIYOR ---

            if (randevu.TarihSaat < DateTime.Now)
            {
                ModelState.AddModelError("TarihSaat", "Geçmiş bir tarihe randevu alamazsınız.");
            }

            var secilenAntrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(a => a.AntrenorId == randevu.AntrenorId);

            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);

            if (secilenAntrenor != null && secilenHizmet != null)
            {
                DateTime baslangicZamani = randevu.TarihSaat;
                DateTime bitisZamani = baslangicZamani.AddMinutes(secilenHizmet.SureDakika);

                // A. SALON ÇALIŞMA SAATİ KONTROLÜ
                if (secilenAntrenor.SporSalonu != null)
                {
                    TimeSpan salonAcilis = secilenAntrenor.SporSalonu.AcilisSaati;
                    TimeSpan salonKapanis = secilenAntrenor.SporSalonu.KapanisSaati;

                    TimeSpan randevuBaslangicSaat = baslangicZamani.TimeOfDay;
                    TimeSpan randevuBitisSaat = bitisZamani.TimeOfDay;

                    if (randevuBaslangicSaat < salonAcilis || randevuBitisSaat > salonKapanis)
                    {
                        ModelState.AddModelError("TarihSaat",
                            $"Spor Salonu kapalı! ({secilenAntrenor.SporSalonu.Ad} Çalışma Saatleri: {salonAcilis:hh\\:mm} - {salonKapanis:hh\\:mm})");
                    }
                }

                // B. ANTRENÖR MESAİ SAATİ KONTROLÜ
                if (ModelState.IsValid)
                {
                    TimeSpan rBaslangic = baslangicZamani.TimeOfDay;
                    TimeSpan rBitis = bitisZamani.TimeOfDay;

                    if (rBaslangic < secilenAntrenor.CalismaBaslangic || rBitis > secilenAntrenor.CalismaBitis)
                    {
                        ModelState.AddModelError("TarihSaat",
                            $"Antrenör bu saatlerde çalışmıyor. (Kişisel Mesai: {secilenAntrenor.CalismaBaslangic:hh\\:mm} - {secilenAntrenor.CalismaBitis:hh\\:mm})");
                    }
                }

                // C. ÇAKIŞMA KONTROLÜ
                if (ModelState.IsValid)
                {
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
            }
            // --- KONTROLLER BİTTİ ---

            if (ModelState.IsValid)
            {
                _context.Add(randevu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var antrenorListesi = _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Select(a => new {
                    a.AntrenorId,
                    AdVeUzmanlik = a.AdSoyad + " (" + a.UzmanlikAlani + ") - " + (a.SporSalonu != null ? a.SporSalonu.Ad : "Salon Atanmamış")
                })
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
        // 4. ADMIN İŞLEMLERİ (ONAYLA / REDDET)
        // -------------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = "Onaylandı";
            _context.Update(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            randevu.Durum = "Reddedildi";
            _context.Update(randevu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: İptal Et (Kullanıcı için)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IptalEt(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && randevu.UyeId != userId)
            {
                return Unauthorized();
            }

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

        // GET: Edit
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu == null) return NotFound();

            var antrenorListesi = _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Select(a => new
                {
                    a.AntrenorId,
                    AdVeUzmanlik = a.AdSoyad + " (" + a.UzmanlikAlani + ") - " + (a.SporSalonu != null ? a.SporSalonu.Ad : "Salon Yok")
                })
                .ToList();

            ViewData["AntrenorId"] = new SelectList(antrenorListesi, "AntrenorId", "AdVeUzmanlik", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);
            ViewData["UyeId"] = new SelectList(_context.Users, "Id", "UserName", randevu.UyeId);
            return View(randevu);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("RandevuId,TarihSaat,Durum,UyeId,AntrenorId,HizmetId")] Randevu randevu)
        {
            if (id != randevu.RandevuId) return NotFound();

            ModelState.Remove("Uye");
            ModelState.Remove("Antrenor");
            ModelState.Remove("Hizmet");

            // --- KONTROLLER BAŞLIYOR ---

            if (randevu.TarihSaat < DateTime.Now)
            {
                ModelState.AddModelError("TarihSaat", "Geçmiş bir tarihe randevu düzenleyemezsiniz.");
            }

            var secilenAntrenor = await _context.Antrenorler
                .Include(a => a.SporSalonu)
                .FirstOrDefaultAsync(a => a.AntrenorId == randevu.AntrenorId);

            var secilenHizmet = await _context.Hizmetler.FindAsync(randevu.HizmetId);

            if (secilenAntrenor != null && secilenHizmet != null)
            {
                DateTime baslangicZamani = randevu.TarihSaat;
                DateTime bitisZamani = baslangicZamani.AddMinutes(secilenHizmet.SureDakika);

                // A. SALON ÇALIŞMA SAATİ KONTROLÜ
                if (secilenAntrenor.SporSalonu != null)
                {
                    TimeSpan salonAcilis = secilenAntrenor.SporSalonu.AcilisSaati;
                    TimeSpan salonKapanis = secilenAntrenor.SporSalonu.KapanisSaati;

                    TimeSpan randevuBaslangicSaat = baslangicZamani.TimeOfDay;
                    TimeSpan randevuBitisSaat = bitisZamani.TimeOfDay;

                    if (randevuBaslangicSaat < salonAcilis || randevuBitisSaat > salonKapanis)
                    {
                        ModelState.AddModelError("TarihSaat",
                            $"Spor Salonu kapalı! ({secilenAntrenor.SporSalonu.Ad} Çalışma Saatleri: {salonAcilis:hh\\:mm} - {salonKapanis:hh\\:mm})");
                    }
                }

                // B. ANTRENÖR MESAİ SAATİ KONTROLÜ
                if (ModelState.IsValid)
                {
                    TimeSpan rBaslangic = baslangicZamani.TimeOfDay;
                    TimeSpan rBitis = bitisZamani.TimeOfDay;

                    if (rBaslangic < secilenAntrenor.CalismaBaslangic || rBitis > secilenAntrenor.CalismaBitis)
                    {
                        ModelState.AddModelError("TarihSaat",
                            $"Antrenör bu saatlerde çalışmıyor. (Kişisel Mesai: {secilenAntrenor.CalismaBaslangic:hh\\:mm} - {secilenAntrenor.CalismaBitis:hh\\:mm})");
                    }
                }

                // C. ÇAKIŞMA KONTROLÜ (Kendi randevusu hariç)
                if (ModelState.IsValid)
                {
                    var cakismanRandevular = await _context.Randevular
                        .Include(r => r.Hizmet)
                        .Where(r => r.AntrenorId == randevu.AntrenorId
                                 && r.TarihSaat.Date == randevu.TarihSaat.Date
                                 && r.Durum != "İptal"
                                 && r.Durum != "Reddedildi"
                                 && r.RandevuId != randevu.RandevuId) // Kendi randevusu hariç
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
            }
            // --- KONTROLLER BİTTİ ---

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

            var antrenorListesi = _context.Antrenorler
                .Include(a => a.SporSalonu)
                .Select(a => new {
                    a.AntrenorId,
                    AdVeUzmanlik = a.AdSoyad + " (" + a.UzmanlikAlani + ") - " + (a.SporSalonu != null ? a.SporSalonu.Ad : "Salon Atanmamış")
                })
                .ToList();

            ViewData["AntrenorId"] = new SelectList(antrenorListesi, "AntrenorId", "AdVeUzmanlik", randevu.AntrenorId);
            ViewData["HizmetId"] = new SelectList(_context.Hizmetler, "HizmetId", "Ad", randevu.HizmetId);
            ViewData["UyeId"] = new SelectList(_context.Users, "Id", "UserName", randevu.UyeId);
            return View(randevu);
        }

        // GET: Delete
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

        // POST: Delete
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
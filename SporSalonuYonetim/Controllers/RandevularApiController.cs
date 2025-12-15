using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Controllers
{
    [Route("api/randevu")]
    [ApiController]
    public class RandevularApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RandevularApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM RANDEVULARI GETİR (Senin Kodun - Aynen Korundu)
        // Link: https://localhost:XXXX/api/randevu/listele
        [HttpGet("listele")]
        public async Task<IActionResult> GetRandevular()
        {
            var randevular = await _context.Randevular
                                   .Include(r => r.Antrenor)
                                   .Include(r => r.Hizmet) // Hizmet bilgisini de ekledik
                                   .ToListAsync();
            return Ok(randevular);
        }

        // 2. ANTRENÖR İSMİNE GÖRE FİLTRELEME (Senin Kodun - Aynen Korundu)
        // Link: https://localhost:XXXX/api/randevu/ara/Ahmet
        [HttpGet("ara/{isim}")]
        public async Task<IActionResult> GetRandevularByHoca(string isim)
        {
            var filtrelenmisListe = await _context.Randevular
                                          .Include(r => r.Antrenor)
                                          .Where(r => r.Antrenor.AdSoyad.Contains(isim))
                                          .ToListAsync();

            if (!filtrelenmisListe.Any())
            {
                return NotFound($"{isim} isminde bir hocaya ait randevu bulunamadı.");
            }

            return Ok(filtrelenmisListe);
        }

        // --- AŞAĞIDAKİLER YENİ EKLENEN GEREKSİNİMLER ---

        // 3. TARİHE GÖRE FİLTRELEME (Hocanın İstediği Madde)
        // Link: https://localhost:XXXX/api/randevu/tarih?tarih=2025-12-16
        [HttpGet("tarih")]
        public async Task<IActionResult> GetByDate(DateTime tarih)
        {
            // Sadece o günün randevularını getirir
            var randevular = await _context.Randevular
                                           .Where(r => r.TarihSaat.Date == tarih.Date)
                                           .Include(r => r.Antrenor)
                                           .Include(r => r.Hizmet)
                                           // JSON döngüsüne girmemesi için temiz bir liste oluşturuyoruz
                                           .Select(r => new
                                           {
                                               r.RandevuId,
                                               Tarih = r.TarihSaat.ToString("dd.MM.yyyy HH:mm"),
                                               Antrenor = r.Antrenor.AdSoyad,
                                               Hizmet = r.Hizmet.Ad,
                                               Durum = r.Durum
                                           })
                                           .ToListAsync();

            if (!randevular.Any())
                return Ok(new { Mesaj = "Bu tarihte kayıtlı randevu yok." });

            return Ok(randevular);
        }

        // 4. ÜYE GEÇMİŞİNİ GETİRME (Hocanın İstediği Diğer Madde)
        // Link: https://localhost:XXXX/api/randevu/uye/UYE_ID_BURAYA
        [HttpGet("uye/{uyeId}")]
        public async Task<IActionResult> GetUyeRandevulari(string uyeId)
        {
            var uyeGecmisi = await _context.Randevular
                                           .Where(r => r.UyeId == uyeId)
                                           .OrderByDescending(r => r.TarihSaat)
                                           .Select(r => new
                                           {
                                               Tarih = r.TarihSaat,
                                               Antrenor = r.Antrenor.AdSoyad,
                                               Hizmet = r.Hizmet.Ad,
                                               Durum = r.Durum
                                           })
                                           .ToListAsync();

            return Ok(uyeGecmisi);
        }
    }
}
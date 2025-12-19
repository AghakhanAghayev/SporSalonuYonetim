using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Controllers
{
    // BURAYI DEĞİŞTİRDİK: Artık adresimiz çok basit
    [Route("api/antrenor")]
    [ApiController]
    public class AntrenorlerApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AntrenorlerApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜMÜNÜ GETİR
        // Link: https://localhost:XXXX/api/antrenor/listele
        [HttpGet("listele")]
        public async Task<IActionResult> GetAntrenorler()
        {
            var antrenorler = await _context.Antrenorler.ToListAsync();
            return Ok(antrenorler);
        }

        // 3. SALON BİLGİLERİNİ GETİR
        [HttpGet("salon/{id}")]
        public async Task<IActionResult> GetSalon(int id)
        {
            var salon = await _context.SporSalonlari.FindAsync(id);
            if (salon == null) return NotFound();
            return Ok(new { salon.AcilisSaati, salon.KapanisSaati });
        }

        // 4. RANDEVU KONTROLÜ
        [HttpPost("kontrol-randevu")]
        public async Task<IActionResult> KontrolRandevu([FromBody] RandevuKontrolModel model)
        {
            var antrenor = await _context.Antrenorler.Include(a => a.SporSalonu).FirstOrDefaultAsync(a => a.AntrenorId == model.AntrenorId);
            var hizmet = await _context.Hizmetler.FindAsync(model.HizmetId);

            if (antrenor == null || hizmet == null) return BadRequest("Geçersiz antrenör veya hizmet.");

            DateTime baslangic = model.TarihSaat;
            DateTime bitis = baslangic.AddMinutes(hizmet.SureDakika);

            var errors = new List<string>();

            // Salon kontrolü
            if (antrenor.SporSalonu != null)
            {
                if (baslangic.TimeOfDay < antrenor.SporSalonu.AcilisSaati || bitis.TimeOfDay > antrenor.SporSalonu.KapanisSaati)
                {
                    errors.Add("Salon kapalı.");
                }
            }

            // Antrenör mesai kontrolü
            if (baslangic.TimeOfDay < antrenor.CalismaBaslangic || bitis.TimeOfDay > antrenor.CalismaBitis)
            {
                errors.Add("Antrenör çalışmıyor.");
            }

            // Çakışma kontrolü
            var cakisman = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.AntrenorId == model.AntrenorId && r.TarihSaat.Date == baslangic.Date && r.Durum != "İptal" && r.Durum != "Reddedildi")
                .ToListAsync();

            foreach (var r in cakisman)
            {
                DateTime rBitis = r.TarihSaat.AddMinutes(r.Hizmet.SureDakika);
                if (baslangic < rBitis && bitis > r.TarihSaat)
                {
                    errors.Add("Antrenör dolu.");
                    break;
                }
            }

            return Ok(new { valid = errors.Count == 0, errors });
        }

        public class RandevuKontrolModel
        {
            public int AntrenorId { get; set; }
            public int HizmetId { get; set; }
            public DateTime TarihSaat { get; set; }
        }
    }
}
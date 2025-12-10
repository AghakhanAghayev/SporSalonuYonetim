using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Controllers
{
    // ADRESİ BASİTLEŞTİRDİK
    [Route("api/randevu")]
    [ApiController]
    public class RandevularApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RandevularApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM RANDEVULARI GETİR
        // Link: https://localhost:XXXX/api/randevu/listele
        [HttpGet("listele")]
        public async Task<IActionResult> GetRandevular()
        {
            var randevular = await _context.Randevular
                                   .Include(r => r.Antrenor) // Antrenör bilgilerini de getir
                                   .ToListAsync();
            return Ok(randevular);
        }

        // 2. LINQ İLE FİLTRELEME (Antrenör İsmine Göre)
        // Örnek Link: https://localhost:XXXX/api/randevu/ara/Ahmet
        [HttpGet("ara/{isim}")]
        public async Task<IActionResult> GetRandevularByHoca(string isim)
        {
            // LINQ SORGUSU BURADA: Antrenörünün adında 'isim' geçenleri filtrele
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
    }
}
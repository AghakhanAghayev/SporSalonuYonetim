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

        // 2. LINQ İLE FİLTRELE (Hocanın İstediği Kritik Madde)
        // Link: https://localhost:XXXX/api/antrenor/uzmanlik/Fitness
        [HttpGet("uzmanlik/{alan}")]
        public async Task<IActionResult> GetByUzmanlik(string alan)
        {
            // İŞTE LINQ SORGUSU BURADA: .Where(...)
            var filtrelenmisListe = await _context.Antrenorler
                                          .Where(a => a.UzmanlikAlani.Contains(alan))
                                          .ToListAsync();

            if (!filtrelenmisListe.Any())
            {
                return NotFound($"{alan} alanında uzman antrenör bulunamadı.");
            }

            return Ok(filtrelenmisListe);
        }
    }
}
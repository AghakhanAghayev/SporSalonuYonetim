using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Data;

namespace SporSalonuYonetim.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RandevularApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RandevularApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. TÜM RANDEVULARI GETİR (Hangi üye hangi hocadan ne zaman randevu almış?)
        // İstek Adresi: GET /api/RandevularApi
        [HttpGet]
        public async Task<IActionResult> GetRandevular()
        {
            // Include kullanarak ilişkili verileri (Antrenör adı vb.) de çekiyoruz.
            var randevular = await _context.Randevular
                                   .Include(r => r.Antrenor) // Antrenör bilgisini dahil et
                                   .ToListAsync();
            return Ok(randevular);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using SporSalonuYonetim.Data;
using SporSalonuYonetim.Models;
using System.Linq;

namespace SporSalonuYonetim.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AntrenorlerApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AntrenorlerApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AntrenorlerApi
        // Bu adres çağrıldığında tüm antrenörleri JSON olarak verir.
        [HttpGet]
        public IActionResult GetAntrenorler()
        {
            var antrenorler = _context.Antrenorler.ToList();
            return Ok(antrenorler);
        }

        // GET: api/AntrenorlerApi/Uzmanlik/Fitness
        // LINQ Sorgusu ile Filtreleme İsteri (Madde 4'teki şart)
        [HttpGet("Uzmanlik/{alan}")]
        public IActionResult GetAntrenorlerByUzmanlik(string alan)
        {
            var filtrelenmis = _context.Antrenorler
                                .Where(a => a.UzmanlikAlani.Contains(alan))
                                .ToList();
            return Ok(filtrelenmis);
        }
    }
}
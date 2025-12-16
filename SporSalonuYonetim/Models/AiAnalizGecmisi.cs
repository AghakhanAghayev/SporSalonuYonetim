using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SporSalonuYonetim.Models
{
    public class AiAnalizGecmisi
    {
        [Key]
        public int Id { get; set; }

        public string KullaniciSorusu { get; set; } // "25 yaş, 180cm, Kilo Vermek..." özeti
        public string AiCevabi { get; set; }        // AI'nın ürettiği program
        public string? ResimUrl { get; set; }       // Oluşturulan görselin linki

        public DateTime Tarih { get; set; } = DateTime.Now;

        // Hangi üyeye ait?
        public string UyeId { get; set; }
        public virtual IdentityUser Uye { get; set; }
    }
}
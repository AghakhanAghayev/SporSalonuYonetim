using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class SporSalonu
    {
        [Key]
        public int SporSalonuId { get; set; }

        [Required]
        [Display(Name = "Salon Adı")]
        public string Ad { get; set; }

        [Display(Name = "Adres")]
        public string Adres { get; set; }

        [Display(Name = "Çalışma Saatleri")]
        public string CalismaSaatleri { get; set; } // Örn: 09:00 - 22:00
    }
}
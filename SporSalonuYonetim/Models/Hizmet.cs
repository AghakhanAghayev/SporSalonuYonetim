using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Hizmet
    {
        [Key]
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [Display(Name = "Hizmet Türü")]
        public string Ad { get; set; } // Örn: Yoga, Pilates, Fitness

        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; }

        [Required]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; } // Hizmetin ücreti

        [Required]
        [Display(Name = "Süre (Dakika)")]
        public int Sure { get; set; } // Hizmetin süresi

        // Bu hizmeti veren antrenörler listesi
        public virtual ICollection<Antrenor> Antrenorler { get; set; }
    }
}
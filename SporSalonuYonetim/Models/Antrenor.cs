using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Antrenor
    {
        [Key]
        public int AntrenorId { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; }

        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; } // Örn: Kilo Verme, Kas Kazanımı

        // Müsaitlik saatleri
        [Display(Name = "Çalışma Başlangıç Saati")]
        public TimeSpan CalismaBaslangic { get; set; }

        [Display(Name = "Çalışma Bitiş Saati")]
        public TimeSpan CalismaBitis { get; set; }

        // Antrenörün verebildiği hizmetler
        public virtual ICollection<Hizmet> Hizmetler { get; set; }

        // Antrenörün randevuları
        public virtual ICollection<Randevu> Randevular { get; set; }
    }
}
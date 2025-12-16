using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Hizmet
    {
        [Key]
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [Display(Name = "Hizmet Türü")]
        public string Ad { get; set; } // Örn: Yoga, Pilates

        // EKSİK OLAN KISIM BURASIYDI 👇
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Ücret alanı zorunludur.")]
        [Range(0, 50000, ErrorMessage = "Ücret 0'dan küçük olamaz.")]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        [Required(ErrorMessage = "Süre alanı zorunludur.")]
        [Range(10, 300, ErrorMessage = "Süre en az 10, en fazla 300 dakika olabilir.")]
        [Display(Name = "Süre (Dakika)")]
        public int SureDakika { get; set; }

        // VE BURASI EKSİKTİ (HATAYI ÇÖZEN SATIR) 👇
        [Display(Name = "Resim URL")]
        public string? ResimUrl { get; set; }
        // ----------------------------------------

        public virtual ICollection<Antrenor>? Antrenorler { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }
    }
}
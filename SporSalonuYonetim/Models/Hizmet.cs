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

        // 1. DEĞİŞİKLİK: Soru işareti (?) ekledik. Artık boş bırakılabilir.
        [Display(Name = "Açıklama")]
        public string? Aciklama { get; set; }

        [Required(ErrorMessage = "Ücret alanı zorunludur.")]
        // 2. DEĞİŞİKLİK: Eksi değer girilmesini engelledik (0 - 50.000 TL arası).
        [Range(0, 50000, ErrorMessage = "Ücret 0'dan küçük olamaz.")]
        [Display(Name = "Ücret (TL)")]
        public decimal Ucret { get; set; }

        [Required(ErrorMessage = "Süre alanı zorunludur.")]
        // 3. DEĞİŞİKLİK: Mantıksız süreleri engelledik (10 dk - 300 dk arası).
        [Range(10, 300, ErrorMessage = "Süre en az 10, en fazla 300 dakika olabilir.")]
        [Display(Name = "Süre (Dakika)")]
        public int SureDakika { get; set; } // İsmini 'Sure'den 'SureDakika'ya çevirdik (View'lerle uyum için)

        // 4. DEĞİŞİKLİK: Soru işareti (?) ekledik. "The Antrenorler field is required" hatasını çözer.
        public virtual ICollection<Antrenor>? Antrenorler { get; set; }
    }
}
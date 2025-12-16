using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class Antrenor : IValidatableObject
    {
        [Key]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Adı Soyadı")]
        public string AdSoyad { get; set; }

        // --- YENİ EKLENEN ALAN ---
        [Required(ErrorMessage = "Lütfen cinsiyet seçiniz.")]
        [Display(Name = "Cinsiyet")]
        public string Cinsiyet { get; set; } // "Erkek" veya "Kadın"
        // -------------------------

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; }

        public int SporSalonuId { get; set; }
        public SporSalonu? SporSalonu { get; set; }

        [Display(Name = "Profil Resmi URL")]
        public string? ResimUrl { get; set; }

        [Required]
        [Display(Name = "Çalışma Başlangıç")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan CalismaBaslangic { get; set; }

        [Required]
        [Display(Name = "Çalışma Bitiş")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan CalismaBitis { get; set; }

        public virtual ICollection<Hizmet>? Hizmetler { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CalismaBitis <= CalismaBaslangic)
            {
                yield return new ValidationResult("Bitiş saati başlangıçtan büyük olmalı.", new[] { "CalismaBitis" });
            }
        }
    }
}
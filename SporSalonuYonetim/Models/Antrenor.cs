using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    // IValidatableObject arayüzünü ekledik: Saatleri birbiriyle kıyaslamak için gerekli.
    public class Antrenor : IValidatableObject
    {
        [Key]
        public int AntrenorId { get; set; }

        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Adı Soyadı")]
        // KONTROL 1: Sadece harfler ve boşluk. Rakam girilirse hata verir.
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$", ErrorMessage = "Ad Soyad alanına rakam veya sembol girilemez.")]
        public string AdSoyad { get; set; }

        [Required(ErrorMessage = "Lütfen bir uzmanlık alanı seçiniz.")]
        [Display(Name = "Uzmanlık Alanı")]
        public string UzmanlikAlani { get; set; }

        // KONTROL 2: Saat Formatı
        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        [Display(Name = "Çalışma Başlangıç Saati")]
        [DataType(DataType.Time)] // HTML'de saat seçici (ikon) çıkmasını sağlar
        public TimeSpan CalismaBaslangic { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [Display(Name = "Çalışma Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan CalismaBitis { get; set; }

        // Yanlarına ? koyarak "Boş olabilir" (Nullable) yapıyoruz
        public virtual ICollection<Hizmet>? Hizmetler { get; set; }
        public virtual ICollection<Randevu>? Randevular { get; set; }

        // KONTROL 3: Mantıksal Saat Kontrolü (Başlangıç > Bitiş Hatası)
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CalismaBitis <= CalismaBaslangic)
            {
                yield return new ValidationResult(
                    "Mesai bitiş saati, başlangıç saatinden sonra olmalıdır.",
                    new[] { "CalismaBitis" });
            }
        }
    }
}
using System.ComponentModel.DataAnnotations;

namespace SporSalonuYonetim.Models
{
    public class SporSalonu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı boş bırakılamaz.")]
        [Display(Name = "Salon Adı")]
        [StringLength(100, ErrorMessage = "Salon adı en fazla 100 karakter olabilir.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Şehir bilgisi zorunludur.")]
        [Display(Name = "Şehir")]
        // ÖZEL KURAL: Sadece harf ve boşluk kabul eder (Rakam giremezsin)
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$", ErrorMessage = "Şehir isminde rakam veya sembol kullanılamaz.")]
        public string Sehir { get; set; }

        [Required(ErrorMessage = "Adres tarifi zorunludur.")]
        [Display(Name = "Adres Tarifi")]
        [MinLength(10, ErrorMessage = "Lütfen daha açıklayıcı bir adres girin (En az 10 karakter).")]
        public string Adres { get; set; }

        [Required(ErrorMessage = "Açılış saati zorunludur.")]
        [Display(Name = "Açılış Saati")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan AcilisSaati { get; set; }

        [Required(ErrorMessage = "Kapanış saati zorunludur.")]
        [Display(Name = "Kapanış Saati")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan KapanisSaati { get; set; }

        // --- HARİTA KOORDİNAT KONTROLLERİ ---
        [Display(Name = "Enlem (Latitude)")]
        // Sadece sayı, nokta ve virgül kabul eder. Harf kabul etmez.
        [RegularExpression(@"^-?([0-9]+([.,][0-9]*)?|[.,][0-9]+)$", ErrorMessage = "Geçerli bir koordinat giriniz (Örn: 40.77). Harf kullanılamaz.")]
        public string? Enlem { get; set; }

        [Display(Name = "Boylam (Longitude)")]
        [RegularExpression(@"^-?([0-9]+([.,][0-9]*)?|[.,][0-9]+)$", ErrorMessage = "Geçerli bir koordinat giriniz (Örn: 30.39). Harf kullanılamaz.")]
        public string? Boylam { get; set; }
        // -------------------------------------

        public ICollection<Antrenor>? Antrenorler { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [Display(Name = "Telefon")]
        [DataType(DataType.PhoneNumber)]
        // ÖZEL KURAL: 05 ile başlamalı ve toplam 11 hane olmalı (Sadece rakam)
        [RegularExpression(@"^(05)[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "Telefon formatı hatalı! (Örn: 05551234567)")]
        public string Telefon { get; set; }

        [Required(ErrorMessage = "Kapasite zorunludur.")]
        [Display(Name = "Kapasite")]
        // ÖZEL KURAL: Eksi değer girilemez.
        [Range(1, 10000, ErrorMessage = "Kapasite 1 ile 10.000 arasında olmalıdır.")]
        public int Kapasite { get; set; }

        [Display(Name = "Resim URL")]
        // URL formatı kontrolü
        [Url(ErrorMessage = "Lütfen geçerli bir internet bağlantısı (URL) giriniz.")]
        public string? ResimUrl { get; set; }
    }
}
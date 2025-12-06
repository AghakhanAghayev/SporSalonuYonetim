using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // IdentityUser için gerekli

namespace SporSalonuYonetim.Models
{
    public class Randevu
    {
        [Key]
        public int RandevuId { get; set; }

        [Required]
        [Display(Name = "Randevu Tarihi ve Saati")]
        public DateTime TarihSaat { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Bekliyor"; // Bekliyor, Onaylandı, İptal [cite: 21]

        // --- İlişkiler ---

        // Randevuyu alan üye (IdentityUser kullanıyoruz)
        public string UyeId { get; set; }
        public virtual IdentityUser Uye { get; set; }

        // Seçilen Antrenör
        public int AntrenorId { get; set; }
        public virtual Antrenor Antrenor { get; set; }

        // Seçilen Hizmet
        public int HizmetId { get; set; }
        public virtual Hizmet Hizmet { get; set; }
    }
}
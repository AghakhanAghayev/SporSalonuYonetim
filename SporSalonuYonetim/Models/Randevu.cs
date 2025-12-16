using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity; // IdentityUser için gerekli

namespace SporSalonuYonetim.Models
{
    public class Randevu
    {
        [Key]
        public int RandevuId { get; set; }

        [Required(ErrorMessage = "Tarih ve saat seçimi zorunludur.")]
        [Display(Name = "Randevu Tarihi ve Saati")]

        // DÜZELTME BURADA: DateTime olduğu için "HH" (Büyük Harf) kullandık.
        // Bu sayede 17:00 olarak görünür. "hh" yaparsan 05:00 görünür.
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime TarihSaat { get; set; }

        [Display(Name = "Durum")]
        public string Durum { get; set; } = "Bekliyor"; // Bekliyor, Onaylandı, İptal

        // --- İlişkiler ---

        // Randevuyu alan üye
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
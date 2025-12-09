using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Models;

namespace SporSalonuYonetim.Data
{
    // DİKKAT: Artık DbContext değil, IdentityDbContext oldu!
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // SENİN MEVCUT TABLOLARIN (Bunlar aynen kalmalı)
        public DbSet<SporSalonu> SporSalonlari { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}
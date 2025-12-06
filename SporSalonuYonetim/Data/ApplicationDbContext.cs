using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SporSalonuYonetim.Models; // Modellerine erişmek için bu satır şart

namespace SporSalonuYonetim.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Veritabanında oluşacak tabloları burada tanımlıyoruz
        public DbSet<SporSalonu> SporSalonlari { get; set; }
        public DbSet<Hizmet> Hizmetler { get; set; }
        public DbSet<Antrenor> Antrenorler { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
    }
}
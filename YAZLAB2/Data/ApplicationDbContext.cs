using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // IdentityDbContext için gerekli
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Models;
using YAZLAB2.Models;
using Microsoft.AspNetCore.Identity;

namespace YAZLAB2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User> // IdentityDbContext'i kullan
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties
        public DbSet<User> Users { get; set; }
        public DbSet<Etkinlik> Etkinlikler { get; set; }
        public DbSet<Katilimci> Katilimcis { get; set; }  // Keep only one DbSet for Katilimci
        public DbSet<Mesaj> Mesajlar { get; set; }
        public DbSet<Puan> Puanlar { get; set; }
        public DbSet<IlgiAlanı> IlgiAlanları { get; set; }
        public DbSet<Kategori> Kategoris { get; set; }

        // Model configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // IdentityDbContext'in OnModelCreating'i çağrılmalı

            // Entity configuration
            modelBuilder.Entity<User>();

            modelBuilder.Entity<Katilimci>()
                .HasKey(k => new { k.KullanıcıId, k.EtkinlikID }); // Composite key for Katilimci
        }
    }
}

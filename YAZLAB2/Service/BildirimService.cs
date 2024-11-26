using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YAZLAB2.Models;
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;

namespace YAZLAB2.Services
{
   
        public class BildirimService
        {
            private readonly ApplicationDbContext _context;

            public BildirimService(ApplicationDbContext context)
            {
                _context = context;
            }

            // Admin'e bildirim ekleme
            public async Task AddBildirimToAdminAsync(string kullaniciId, int etkinlikId)
            {
                // Etkinlik bilgisini al
                var etkinlik = await _context.Etkinlikler
                    .Include(e => e.UserId)  // Etkinliği oluşturan kullanıcıyı da çekiyoruz
                    .FirstOrDefaultAsync(e => e.EtkinlikId == etkinlikId);

                if (etkinlik == null)
                {
                    return;  // Etkinlik bulunamazsa, işlem yapma
                }

                // Admin'e gösterilecek bildirim metni
                var bildirim = new Bildirim
                {
                    KullanıcıId = kullaniciId, // Admin'e bildirim
                    EtkinlikId = etkinlikId,
                    BildirimTarih = DateTime.Now
                };

                // Bildirim mesajı ile etkinlik hakkında bilgi ekliyoruz
                var mesaj = $"Yeni bir etkinlik oluşturuldu! \n" +
                            $"Etkinlik Adı: {etkinlik.EtkinlikAdi} \n" +
                           /// $"Oluşturan Kullanıcı: {etkinlik.User.UserName} \n" +
                            $"Etkinlik Tarihi: {etkinlik.Tarih.ToString("dd/MM/yyyy")} \n" +
                            $"Etkinlik Saati: {etkinlik.Saat.ToString(@"hh\:mm")}";

                // Bildirimi kaydet
                _context.Bildirimler.Add(bildirim);
                await _context.SaveChangesAsync();
            }
        

        public async Task AddBildirimAsync(string kullaniciId, int etkinlikId)
        {
            var bildirim = new Bildirim
            {
                KullanıcıId = kullaniciId,
                EtkinlikId = etkinlikId,
                BildirimTarih = DateTime.Now
            };

            _context.Bildirimler.Add(bildirim);
            await _context.SaveChangesAsync();
        }
        // Kullanıcıya ait bildirimleri listeleme
        public async Task<List<Bildirim>> GetBildirimlerByUserIdAsync(string kullaniciId)
        {
            return await _context.Bildirimler
                .Include(b => b.Etkinlik)
                .Where(b => b.KullanıcıId == kullaniciId)
                .OrderByDescending(b => b.BildirimTarih)
                .ToListAsync();
        }

        // Belirli bir bildirimi silme
        public async Task DeleteBildirimAsync(int bildirimId)
        {
            var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
            if (bildirim != null)
            {
                _context.Bildirimler.Remove(bildirim);
                await _context.SaveChangesAsync();
            }
        }
    }
}

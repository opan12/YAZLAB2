namespace Yazlab__2.Core.Service
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using YAZLAB2.Data;
    using YAZLAB2.Models;

    public class MesajServisi
    {
        private readonly ApplicationDbContext _context;

        public MesajServisi(ApplicationDbContext context)
        {
            _context = context;
        }

        // Yorum ekleme
        public async Task<bool> YorumEkle(string gondericiID, int etkinlikId, string mesajMetni)
        {
            
            try
            {
                var mesaj = new Mesaj
                {
                    GondericiID = gondericiID,
                    EtkinlikId = etkinlikId,
                    MesajMetni = mesajMetni,
                    GonderimZamani = DateTime.UtcNow
                };

                _context.Mesajlar.Add(mesaj);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                // Hata kaydını loglayın
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }


        // Etkinlik yorumlarını getirme
        public async Task<List<Mesaj>> EtkinlikYorumlari(int etkinlikId)
        {
            return await _context.Mesajlar
                .Where(m => m.EtkinlikId == etkinlikId)
                .ToListAsync();
        }

        // Mesaj gönderme
        public async Task<bool> MesajGonder(string gondericiID, string aliciID, string mesajMetni)
        {
            var mesaj = new Mesaj
            {
                GondericiID = gondericiID,
                AliciID = aliciID,
                MesajMetni = mesajMetni,
                GonderimZamani = DateTime.UtcNow
            };

            _context.Mesajlar.Add(mesaj);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        // Kullanıcı mesajlarını getirme
        public async Task<List<Mesaj>> KullaniciMesajlari(string kullaniciId)
        {
            return await _context.Mesajlar
                .Where(m => m.GondericiID == kullaniciId)
                .ToListAsync();
        }
    }
}

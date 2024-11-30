using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using YAZLAB2.Data;
using iText.Commons.Actions.Contexts;

namespace Yazlab__2.Service
{
    public class PuanHesaplayiciService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PuanHesaplayiciService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Katılım Puanı Hesaplama
        public async Task<int> HesaplaKatilimPuan(string userId)
        {
            // Kullanıcının katıldığı etkinlikleri alıyoruz
            var katildigiEtkinlikler = await _context.Katilimcis
                .Where(k => k.KullanıcıId == userId)
                .ToListAsync();

            // Eğer kullanıcı ilk kez katılıyorsa 20 puan verelim
            if (katildigiEtkinlikler.Count == 1)
            {
                return 20; // İlk etkinliğe katılan kullanıcılara özel puan
            }

            // Katıldıkları her etkinlik için 10 puan veriyoruz
            return katildigiEtkinlikler.Count * 10;
        }


        // Etkinlik Oluşturma Puanı Hesaplama
        public async Task<int> HesaplaOlusturmaPuan(string userId)
        {
            // Kullanıcının oluşturduğu etkinlikleri alıyoruz
            var olusturduguEtkinlikler = await _context.Etkinlikler
                .Where(e => e.UserId == userId)
                .ToListAsync();

            // Oluşturduğu her etkinlik için 15 puan veriyoruz
            return olusturduguEtkinlikler.Count * 15;
        }

        // Bonus Puan Hesaplama (Örneğin, etkinlik türüne veya başka kriterlere göre bonus verilebilir)
        public async Task<int> HesaplaBonusPuan(string userId)
        {
            // Örnek: Kullanıcının etkinliklerinde belirli bir kategoriden bonus puan verelim
            var bonusPuan = await _context.Etkinlikler
                .Where(e => e.UserId == userId && e.KategoriId == 1) // Örneğin, kategori ID'si 1 olan etkinliklerden bonus
                .CountAsync();

            return bonusPuan * 5; // Her bonus etkinlik için 5 puan verelim
        }

        // Toplam Puan Hesaplama
        public async Task<int> HesaplaToplamPuan(string userId)
        {
            // Katılım, oluşturma ve bonus puanları hesapla
            int katilimPuan = await HesaplaKatilimPuan(userId);
            int olusturmaPuan = await HesaplaOlusturmaPuan(userId);
            int bonusPuan = await HesaplaBonusPuan(userId);

            // Toplam puanı döndür
            return katilimPuan + olusturmaPuan + bonusPuan;
        }


        public async Task KaydetPuan(string userId, int puanDegeri) // public erişim düzeyi
        {
            var yeniPuan = new Puan
            {
                KullaniciID = userId,
                PuanDegeri = puanDegeri,
                KazanilanTarih = DateTime.Now // Puanın kazanıldığı tarihi şu an olarak ayarlıyoruz
            };

            // Puanı veritabanına ekliyoruz
            await _context.Puanlar.AddAsync(yeniPuan);
            await _context.SaveChangesAsync(); // Değişiklikleri kaydediyoruz
        }
    }
}
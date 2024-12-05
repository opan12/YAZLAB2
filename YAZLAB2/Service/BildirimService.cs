using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YAZLAB2.Models;
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;
using Microsoft.AspNetCore.Identity;

namespace YAZLAB2.Services
{

    public class BildirimService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public BildirimService(ApplicationDbContext context, UserManager<User> userManager)
        {
            _userManager = userManager;

            _context = context;

        }
        public async Task<List<Bildirim>> GetKullaniciBildirimlerAsync(string userId)
        {
            return await _context.Bildirimler.Where(b => b.KullanıcıId == userId).ToListAsync();
        }

        public async Task<List<Bildirim>> GetAdminBildirimlerAsync()
        {
            return await _context.Bildirimler.Where(b => b.IsAdminNotification).ToListAsync();
        }
        public async Task DeleteBildirimAsync(int bildirimId)
        {
            var bildirim = await _context.Bildirimler.FindAsync(bildirimId);
            if (bildirim != null)
            {
                _context.Bildirimler.Remove(bildirim);
                await _context.SaveChangesAsync();
            }
        }



        public async Task AddBildirimAsync(string kullaniciId, int etkinlikId, string islemTuru, bool isAdminNotification)
        {
            var etkinlik = await _context.Etkinlikler.FirstOrDefaultAsync(e => e.EtkinlikId == etkinlikId);
            if (etkinlik == null) return;

            var user = await _userManager.FindByIdAsync(etkinlik.UserId);
            var olusturanKullanici = user != null ? user.UserName : "Bilinmiyor";

            var mesaj = $"Etkinlik İşlemi: {islemTuru} \n" +
                        $"Etkinlik Adı: {etkinlik.EtkinlikAdi} \n" +
                        $"Oluşturan Kullanıcı: {olusturanKullanici} \n" +
                        $"Etkinlik Tarihi: {etkinlik.Tarih:dd/MM/yyyy} \n" +
                        $"İşlem Tarihi: {DateTime.Now:dd/MM/yyyy HH:mm}";

            var bildirim = new Bildirim
            {
                KullanıcıId = kullaniciId,
                EtkinlikId = etkinlikId,
                BildirimTarih = DateTime.Now,
                Mesaj = mesaj,
                IsAdminNotification = isAdminNotification 
            };

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

    }
}

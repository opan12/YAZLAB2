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
        public async Task<int> HesaplaKatilimPuan(string userId)
        {
            var katildigiEtkinlikler = await _context.Katilimcis
                .Where(k => k.KullanıcıId == userId)
                .ToListAsync();

            if (katildigiEtkinlikler.Count == 1)
            {
                return 20;
            }

            return katildigiEtkinlikler.Count * 10;
        }


        public async Task<int> HesaplaOlusturmaPuan(string userId)
        {
            var olusturduguEtkinlikler = await _context.Etkinlikler
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return olusturduguEtkinlikler.Count * 15;
        }

        public async Task<int> HesaplaBonusPuan(string userId)
        {
            var bonusPuan = await _context.Etkinlikler
                .Where(e => e.UserId == userId && e.KategoriId == 1) 
                .CountAsync();

            return bonusPuan * 5;
        }

        public async Task<int> HesaplaToplamPuan(string userId)
        {
            int katilimPuan = await HesaplaKatilimPuan(userId);
            int olusturmaPuan = await HesaplaOlusturmaPuan(userId);
            int bonusPuan = await HesaplaBonusPuan(userId);

            return katilimPuan + olusturmaPuan + bonusPuan;
        }


        public async Task KaydetPuan(string userId, int puanDegeri) 
        {
            var yeniPuan = new Puan
            {
                KullaniciID = userId,
                PuanDegeri = puanDegeri,
                KazanilanTarih = DateTime.Now 
            };

            await _context.Puanlar.AddAsync(yeniPuan);
            await _context.SaveChangesAsync(); 
        }
    }
}
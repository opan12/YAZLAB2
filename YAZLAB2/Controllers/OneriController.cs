using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YAZLAB2.Models;
using System.Security.Claims;
using Yazlab__2.Service;
using YAZLAB2.Data;

namespace Yazlab__2.Controllers
{
   
    [Authorize]

    public class OneriController : ControllerBase
    {
        private readonly EtkinlikOnerisiServisi _etkinlikOnerisiServisi;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public OneriController(
            EtkinlikOnerisiServisi etkinlikOnerisiServisi,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            _etkinlikOnerisiServisi = etkinlikOnerisiServisi;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        public async Task<List<Etkinlik>> OneriGetir(string kullaniciId)
        {
            // Kullanıcının katıldığı etkinlik kategorilerinin katılım sayısını hesapla
            var kategoriKatilimSayilari = await _context.Katilimcis
                .Where(k => k.KullanıcıId == kullaniciId)
                .Join(
                    _context.Etkinlikler,
                    katilimci => katilimci.EtkinlikID,
                    etkinlik => etkinlik.EtkinlikId,
                    (katilimci, etkinlik) => etkinlik.KategoriId
                )
                .GroupBy(kategoriId => kategoriId)
                .Select(g => new { KategoriId = g.Key, KatilimSayisi = g.Count() })
                .ToDictionaryAsync(x => x.KategoriId, x => x.KatilimSayisi);

            // İlgi alanlarına göre kategorileri al
            var ilgiAlanıKategoriler = await _context.IlgiAlanları
                .Where(i => i.KullanıcıId == kullaniciId)
                .Select(i => i.KategoriId)
                .ToListAsync();

            // Kullanıcının daha önce katıldığı etkinlik ID'lerini al
            var katildigiEtkinlikIds = await _context.Katilimcis
                .Where(k => k.KullanıcıId == kullaniciId)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

            // Öneri listesini oluştur
            var oneriEtkinlikler = await _context.Etkinlikler
                .AsNoTracking()
                .Where(e =>
                    (ilgiAlanıKategoriler.Contains(e.KategoriId) || kategoriKatilimSayilari.ContainsKey(e.KategoriId)) &&
                    !katildigiEtkinlikIds.Contains(e.EtkinlikId) &&
                    e.OnayDurumu == true)
                .ToListAsync();

            // Etkinlikleri daha çok katıldığı kategorilere göre önceliklendir
            var siralanmisEtkinlikler = oneriEtkinlikler
                .OrderByDescending(e => kategoriKatilimSayilari.ContainsKey(e.KategoriId) ? kategoriKatilimSayilari[e.KategoriId] : 0) // Katılım sayısına göre sıralama
                .ThenBy(e => e.Tarih) // İkinci kriter olarak tarih sıralaması
                .ToList();

            return siralanmisEtkinlikler;
        }

        /*
        public async Task<List<Etkinlik>> OneriGetir(string kullaniciId)
        {
            var tumKategoriler = await _context.IlgiAlanları
                .Where(i => i.KullanıcıId == kullaniciId)
                .Select(i => i.KategoriId)
                .Union(
                    _context.Katilimcis
                    .Where(k => k.KullanıcıId == kullaniciId)
                    .Join(
                        _context.Etkinlikler,
                        katilimci => katilimci.EtkinlikID,
                        etkinlik => etkinlik.EtkinlikId,
                        (katilimci, etkinlik) => etkinlik.KategoriId
                    )
                )
                .Distinct()
                .ToListAsync();

            var katildigiEtkinlikIds = await _context.Katilimcis
                .Where(k => k.KullanıcıId == kullaniciId)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

            var oneriEtkinlikler = await _context.Etkinlikler
                .AsNoTracking()
                .Where(e =>
                    tumKategoriler.Contains(e.KategoriId) &&
                    !katildigiEtkinlikIds.Contains(e.EtkinlikId) &&
                    e.OnayDurumu == true)
                .ToListAsync();

            return oneriEtkinlikler;
        }
        */

    }
}

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
            // Kullanıcının ilgi alanlarına ait kategori ID'lerini al
            var ilgiAlanıKategoriler = await _context.IlgiAlanları
                .Where(i => i.KullanıcıId == kullaniciId)
                .Select(i => i.KategoriId)
                .ToListAsync();

            // Kullanıcının katıldığı etkinliklerden elde edilen kategori ID'lerini al
            var katilimciKategoriler = await _context.Katilimcis
                .Where(k => k.KullanıcıId == kullaniciId)
                .Join(
                    _context.Etkinlikler,
                    katilimci => katilimci.EtkinlikID,
                    etkinlik => etkinlik.EtkinlikId,
                    (katilimci, etkinlik) => etkinlik.KategoriId
                )
                .Distinct()
                .ToListAsync();

            // İlgi alanları ve katıldığı etkinlik kategorilerini birleştir
            var tumKategoriler = ilgiAlanıKategoriler
                .Union(katilimciKategoriler)
                .Distinct()
                .ToList();

            // Kullanıcının daha önce katıldığı etkinlik ID'lerini al
            var katildigiEtkinlikIds = await _context.Katilimcis
                .Where(k => k.KullanıcıId == kullaniciId)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

            // İlgi alanlarına ve kategorilere göre önerilen etkinlikleri filtrele
            var oneriEtkinlikler = await _context.Etkinlikler
                .AsNoTracking()
                .Where(e =>
                    tumKategoriler.Contains(e.KategoriId) && // İlgi alanına veya katıldığı kategorilere uygun etkinlik
                    !katildigiEtkinlikIds.Contains(e.EtkinlikId) && // Daha önce katılmadığı etkinlik
                    e.OnayDurumu == true) // Onaylanmış etkinlikler
                .ToListAsync();

            return oneriEtkinlikler;
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

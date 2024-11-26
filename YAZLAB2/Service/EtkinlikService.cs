namespace Yazlab__2.Service
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using YAZLAB2.Models;

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using YAZLAB2.Data;

    public class EtkinlikService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EtkinlikService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> CheckEtkinlikConflict(EtkinlikDto yeniEtkinlik)
        {
            var currentUserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            if (currentUserName == null)
            {
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == currentUserName);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
            }

            var mevcutEtkinlikler = await _context.Etkinlikler
                .Where(e => e.UserId == user.Id && e.Tarih.Date == yeniEtkinlik.Tarih.Date)
                .ToListAsync();

            foreach (var etkinlik in mevcutEtkinlikler)
            {
                var etkinlikBitisSaati = etkinlik.Tarih.Add(etkinlik.EtkinlikSuresi);
                var yeniEtkinlikBitisSaati = yeniEtkinlik.Tarih.Add(yeniEtkinlik.EtkinlikSuresi);

                if (yeniEtkinlik.Tarih < etkinlikBitisSaati && yeniEtkinlikBitisSaati > etkinlik.Tarih)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> CreateEtkinlik(Etkinlik yeniEtkinlik)
        {
            var etkinlikDto = new EtkinlikDto
            {
                EtkinlikAdi = yeniEtkinlik.EtkinlikAdi,
                Konum = yeniEtkinlik.Konum,
                Aciklama = yeniEtkinlik.Aciklama,
                Tarih = yeniEtkinlik.Tarih,
                EtkinlikSuresi = yeniEtkinlik.EtkinlikSuresi
            };

            if (await CheckEtkinlikConflict(etkinlikDto))
            {
                return false;
            }

            _context.Etkinlikler.Add(yeniEtkinlik);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

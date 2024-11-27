using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;
using YAZLAB2.Models;

public class EtkinlikOnerisiServisi
{
    private readonly ApplicationDbContext _context;

    public EtkinlikOnerisiServisi(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<Etkinlik>> OneriGetir(string kullaniciId)
    {
        var kullaniciIlgiAlanlari = await _context.IlgiAlanları
            .Where(i => i.KullanıcıId == kullaniciId)
            .Select(i => i.KategoriId)
            .ToListAsync();

        var katildigiKategoriIds = await _context.Katilimcis
            .Where(k => k.KullanıcıId == kullaniciId)
            .Join(
                _context.Etkinlikler,
                katilimci => katilimci.EtkinlikID,
                etkinlik => etkinlik.EtkinlikId,
                (katilimci, etkinlik) => etkinlik.KategoriId
            )
            .Distinct()
            .ToListAsync();

        var tumKategoriler = kullaniciIlgiAlanlari.Union(katildigiKategoriIds).ToList();

        var katildigiEtkinlikIds = await _context.Katilimcis
            .Where(k => k.KullanıcıId == kullaniciId)
            .Select(k => k.EtkinlikID)
            .ToListAsync();

        var oneriEtkinlikler = await _context.Etkinlikler
            .Where(e =>
                tumKategoriler.Contains(e.KategoriId) && 
                !katildigiEtkinlikIds.Contains(e.EtkinlikId) && 
                e.OnayDurumu == true 
            )
            .ToListAsync();

        return oneriEtkinlikler;
    }
}

using System.Collections.Generic;
using YAZLAB2.Models;
using YAZLAB2.Data;

public class EtkinlikOnerisiServisi
{
    private readonly ApplicationDbContext _context;

    public EtkinlikOnerisiServisi(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Etkinlik> OneriGetir(string kullaniciId)
    {
        // Kullanıcının katıldığı etkinliklerin ID'lerini alıyoruz
        var katilimlar = _context.Katilimcis
            .Where(k => k.KullanıcıId == kullaniciId)
            .ToList();

        var katilimIds = katilimlar.Select(k => k.EtkinlikID).ToList();

        // Kullanıcının katıldığı etkinliklerin kategorilerini sayarak en sık katıldıklarını buluyoruz
        var kategoriler = katilimlar
            .GroupBy(k => k.EtkinlikID)  // EtkinlikID'ye göre gruplama yapıyoruz
            .Select(g => new
            {
                EtkinlikId = g.Key,
                KatilimSayisi = g.Count()
            })
            .OrderByDescending(g => g.KatilimSayisi) // En sık katıldığı etkinlikleri sıralıyoruz
            .ToList();

        // En sık katıldığı etkinliklerin ID'lerini alıyoruz
        var sıkKatilinanEtkinlikler = kategoriler.Select(k => k.EtkinlikId).ToList();

        // Kullanıcının ilgi alanını alıyoruz
        var kullaniciIlgiAlani = _context.IlgiAlanları
            .Where(i => i.KullanıcıId == kullaniciId)
            .Select(i => i.KategoriId)  // Kullanıcının ilgisini gösteren kategori ID'lerini alıyoruz
            .ToList();

        // İlgi alanına göre etkinlikleri filtreliyoruz
        var ilgiAlaniOnerileri = _context.Etkinlikler
            .Where(e => kullaniciIlgiAlani.Contains(e.KategoriId))  // Etkinliklerin kategorisini kullanıcının ilgisiyle karşılaştırıyoruz
            .ToList();

        // Katılım geçmişine göre etkinlikleri sıralıyoruz
        var etkinlikler = _context.Etkinlikler
            .Where(e => sıkKatilinanEtkinlikler.Contains(e.EtkinlikId))
            .ToList();

        var oneriListesi = new List<Etkinlik>();

        // Önce ilgi alanı uyumlu etkinlikleri ekliyoruz
        oneriListesi.AddRange(ilgiAlaniOnerileri);

        // Daha sonra katılım geçmişine dayalı önerileri ekliyoruz
        foreach (var etkinlik in etkinlikler)
        {
            // Kategorisi aynı olan etkinlikleri ancak kullanıcı daha önce katılmamışsa öneriyoruz
            var kategoriyeGoreOneri = _context.Etkinlikler
                .Where(e => e.KategoriId == etkinlik.KategoriId && !katilimIds.Contains(e.EtkinlikId))
                .ToList();
            oneriListesi.AddRange(kategoriyeGoreOneri);
        }

        // Önerilerdeki yinelenen etkinlikleri temizliyoruz
        var uniqueOneriler = oneriListesi
            .GroupBy(e => e.EtkinlikId)  // Etkinlik ID'sine göre grupluyoruz
            .Select(g => g.First())  // Her gruptan ilkini alıyoruz (tekil etkinlik)
            .ToList();

        return uniqueOneriler;
    }
}

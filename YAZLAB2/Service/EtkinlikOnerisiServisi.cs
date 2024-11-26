using YAZLAB2.Data;
using YAZLAB2.Models;

public class EtkinlikOnerisiServisi
{
    private readonly ApplicationDbContext _context;

    public EtkinlikOnerisiServisi(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Etkinlik> OneriGetir(string kullaniciId)
    {
        // Kullanıcının ilgi alanlarını alıyoruz
        var kullaniciIlgiAlani = _context.IlgiAlanları
            .Where(i => i.KullanıcıId == kullaniciId)
            .Select(i => i.KategoriId)  // Kullanıcının ilgisini gösteren kategori ID'lerini alıyoruz
            .ToList();

        // Kullanıcının katıldığı etkinliklerin ID'lerini alıyoruz
        var kullaniciKatilimlar = _context.Katilimcis
            .Where(k => k.KullanıcıId == kullaniciId)
            .Select(k => k.EtkinlikID)  // Kullanıcının katıldığı etkinlik ID'leri
            .ToList();

        // Eğer kullanıcının katıldığı etkinlikler yoksa, yalnızca ilgi alanına göre etkinlik öneriyoruz
        if (kullaniciKatilimlar.Count == 0)
        {
            // Kullanıcının ilgi alanına göre etkinlikleri filtreliyoruz
            var ilgiAlaniOnerileri = _context.Etkinlikler
                .Where(e => kullaniciIlgiAlani.Contains(e.KategoriId))  // Etkinliklerin kategorisini kullanıcının ilgisiyle karşılaştırıyoruz
                .ToList();

            return ilgiAlaniOnerileri;  // Sadece ilgi alanına göre önerilen etkinlikler
        }
        else
        {
            // Kullanıcının katıldığı etkinliklerin dışındaki etkinlikleri öneriyoruz
            var katilimVeIlgiAlaniOnerileri = _context.Etkinlikler
                .Where(e => kullaniciIlgiAlani.Contains(e.KategoriId) && kullaniciKatilimlar.Contains(e.EtkinlikId))  // Hem katılım olmayan hem de ilgi alanına uyan etkinlikler
                .ToList();

            return katilimVeIlgiAlaniOnerileri;  // Hem ilgi alanına hem de katılım durumu olmayan etkinliklere göre önerilen etkinlikler
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YAZLAB2.Models;
using YAZLAB2.Data;
using Bogus;
using Bogus.DataSets;

public class FakeController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public FakeController(UserManager<User> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _context = context;
        _roleManager = roleManager;
    }

    // View to show the form for generating fake data
    public IActionResult GenerateFakeDataForm()
    {
        return View();
    }

    public async Task<IActionResult> GenerateFakeData(int count = 2)
    {
        var faker = new Faker("tr");
        var fakeUsers = new List<User>();

        // İlçe bilgileri
        var districts = new List<(string District, double Lat, double Lng)>
{
    ("Kadıköy", 40.9939, 29.1043),
    ("Üsküdar", 41.0245, 29.0250),
    ("Beşiktaş", 41.0438, 29.0027),
    ("Beyoğlu", 41.0329, 28.9770),
    ("Maltepe", 40.9834, 29.1143),
    ("Esenyurt", 41.0382, 28.6093),
    // Ankara
    ("Çankaya", 39.9334, 32.8597),
    ("Keçiören", 39.9932, 32.8636),
    ("Sincan", 39.8220, 32.6593),
    ("Etimesgut", 39.9358, 32.5845),
    // İzmir
    ("Konak", 38.4192, 27.1287),
    ("Karşıyaka", 38.4423, 27.1188),
    ("Bornova", 38.4197, 27.2153),
    ("Buca", 38.4195, 27.1449),
    ("Çiğli", 38.5095, 27.0225),
    // Bursa
    ("Osmangazi", 40.1826, 29.0662),
    ("Nilüfer", 40.2103, 29.0574),
    ("Gemlik", 40.4330, 29.0937),
    ("İnegöl", 40.1013, 29.5087),
    // Antalya
    ("Muradpaşa", 36.8957, 30.6952),
    ("Kepez", 36.9564, 30.6781),
    ("Antalya", 36.8841, 30.7056),
    ("Alanya", 36.5434, 31.9995),            
    // Samsun
    ("Atakum", 41.3255, 36.2964),
    // Konya
    ("Selçuklu", 37.8656, 32.4770),
    ("Meram", 37.9170, 32.4851),
    ("Karatay", 37.8706, 32.4840),
    // Eskişehir
    ("Tepebaşı", 39.7767, 30.5274),
    ("Odunpazarı", 39.7692, 30.5176),
    // Kocaeli
    ("İzmit", 40.7402, 29.9402),
    ("Başiskele", 40.7230, 29.8456),
    ("Gölcük", 40.6826, 29.8743),

};

        // Seçilen ilçeleri belirleyin (örnek)
        var selectedDistricts = new List<string>
{

    "Kadıköy",
    "Üsküdar",
    "Beşiktaş",
    "Beyoğlu",
    "Maltepe",
    "Esenyurt",
    // Ankara
    "Çankaya",
    "Keçiören",
    "Sincan",
    "Etimesgut",
    // İzmir
    "Konak",
    "Karşıyaka",
    "Bornova",
    "Buca",
    "Çiğli",
    // Bursa
    "Osmangazi",
    "Nilüfer",
    "Gemlik",
    "İnegöl",
    // Antalya
    "Muradpaşa",
    "Kepez",
    "Antalya",
    "Alanya",
    // Samsun
    "Atakum",

    "Tepebaşı",
    "Odunpazarı",

    "İzmit",
    "Başiskele",
    "Gölcük",

};


        foreach (var district in selectedDistricts)
        {
            // Seçilen ilçenin mevcut olup olmadığını kontrol et
            var districtInfo = districts.FirstOrDefault(d => d.District == district);

            if (districtInfo.Equals(default))
            {
                throw new ArgumentException($"'{district}' ilçesi bulunamadı.");
            }

            for (int i = 0; i < count; i++)
            {
                var fakeUser = new User
                {
                    UserName = faker.Internet.UserName(),
                    Ad = faker.Name.FirstName(),
                    Soyad = faker.Name.LastName(),
                    Email = faker.Internet.Email(),
                    Konum = district,
                    TelefonNumarasi = faker.Phone.PhoneNumber(),
                    DogumTarihi = faker.Date.Past(30, DateTime.Now.AddYears(-18)),
                    Cinsiyet = faker.PickRandom(new[] { "Erkek", "Kadın" }),
                    ProfilFoto = faker.Image.PicsumUrl(),
                    Lat = Math.Round(districtInfo.Lat, 6),
                    Lng = Math.Round(districtInfo.Lng, 6)
                };

                fakeUsers.Add(fakeUser);
            }
        }

        var roleExist = await _roleManager.RoleExistsAsync("User");
        if (!roleExist)
        {
            var role = new IdentityRole("User");
            await _roleManager.CreateAsync(role);
        }

        foreach (var user in fakeUsers)
        {
            var result = await _userManager.CreateAsync(user, "Test1234!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await CreateRandomInterestAreasForUser(user);
                await CreateRandomEventsForUser(user);
            }
            else
            {
                return BadRequest($"Kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        return RedirectToAction("GenerateFakeDataForm");
    }



    public async Task CreateRandomInterestAreasForUser(User user)
    {
        // Varsayılan kategoriler
        var defaultCategories = new List<Kategori>
{
    new Kategori { KategoriAdi = "Spor" },
    new Kategori { KategoriAdi = "Sanat" },
    new Kategori { KategoriAdi = "Teknoloji" },
    new Kategori { KategoriAdi = "Müzik" },
    new Kategori { KategoriAdi = "Eğitim" },
    new Kategori { KategoriAdi = "Tarih" },
    new Kategori { KategoriAdi = "Sağlık" },
    new Kategori { KategoriAdi = "Yemek" },
    new Kategori { KategoriAdi = "Moda" },
    new Kategori { KategoriAdi = "Doğa" },
    new Kategori { KategoriAdi = "İş" },
    new Kategori { KategoriAdi = "Seyahat" },
    new Kategori { KategoriAdi = "Kitap" },
    new Kategori { KategoriAdi = "Sinema" },
    new Kategori { KategoriAdi = "Girişimcilik" }
};

        // Eksik kategorileri veritabanına ekle
        foreach (var category in defaultCategories)
        {
            if (!_context.Kategoris.Any(k => k.KategoriAdi == category.KategoriAdi))
            {
                _context.Kategoris.Add(category);
            }
        }
        await _context.SaveChangesAsync();

        // Tüm kategorileri veritabanından çek
        var allCategories = await _context.Kategoris.ToListAsync();

        // Kullanıcıya rastgele 3 ilgi alanı ata
        var randomCategories = allCategories.OrderBy(c => Guid.NewGuid()).Take(3).ToList();

        foreach (var category in randomCategories)
        {
            // İlgi alanını oluştur
            var interest = new IlgiAlanı
            {
                KullanıcıId = user.Id,
                KategoriId = category.KategoriId
            };

            _context.IlgiAlanları.Add(interest);
        }

        // Veritabanına kaydet
        await _context.SaveChangesAsync();
    }


    //private async Task CreateRandomInterestAreasForUser(User user)
    //{
    //    var allCategories = await _context.Kategoris.ToListAsync();
    //    var randomCategories = allCategories.OrderBy(c => Guid.NewGuid()).Take(3).ToList();

    //    foreach (var category in randomCategories)
    //    {
    //        var interest = new IlgiAlanı
    //        {
    //            KullanıcıId = user.Id,
    //            KategoriId = category.KategoriId
    //        };

    //        _context.IlgiAlanları.Add(interest);
    //    }

    //    await _context.SaveChangesAsync();
    //}

    public async Task CreateRandomEventsForUser(User user)
    {
        var faker = new Faker("tr");
        var etkinlikler = new List<Etkinlik>();
        var bildirimler = new List<Bildirim>();
        var users = await _userManager.Users.ToListAsync(); // Kullanıcı listesini al


        // Aralık 2024 için rastgele bir tarih oluşturuyoruz
        DateTime randomDateInDecember = faker.Date.Between(
            new DateTime(2024, 12, 1), // Aralık 2024 başı
            new DateTime(2024, 12, 31) // Aralık 2024 sonu
        );

        // Rastgele bir saat (00:00 ile 23:59 arasında)
        TimeSpan randomTime = new TimeSpan(faker.Random.Number(0, 23), faker.Random.Number(0, 59), 0);
        var userInterestAreas = await _context.IlgiAlanları
            .Where(ia => ia.KullanıcıId == user.Id)
            .ToListAsync();

        if (!userInterestAreas.Any())
        {
            return;
        }

        var etkinlikAdlariDict = new Dictionary<int, string[]>
    {
        { 1, new[] { "Yaz Festivali", "Çocuk Şenliği" } },
        { 2, new[] { "Sanat Sergisi", "Tiyatro Gösterisi" } },
        { 3, new[] { "Teknoloji Konferansı", "Müzik Dinletisi" } },
        { 4, new[] { "Gastronomi Günü", "Kitap Fuarı" } },
        { 5, new[] { "Spor Turnuvası", "Film Gösterimi" } }
    };

        var etkinlikAciklamalari = new[]
        {
        "Yaz mevsimini kutlamak için düzenlenen bir festival.",
        "Yerel sanatçıların eserlerinin sergilendiği bir etkinlik.",
        "Teknolojideki son gelişmelerin tartışılacağı bir konferans.",
        "Yerel sanatçıların performans sergilediği bir müzik dinletisi.",
        "En yeni kitapların tanıtıldığı bir fuar.",
        "Yerli mutfağın tanıtıldığı bir gastronomi etkinliği.",
        "Çocuklar için eğlenceli aktivitelerin yer aldığı bir şenlik.",
        "Farklı spor branşlarında yarışmaların yapıldığı bir turnuva.",
        "Yeni çıkan filmlerin gösterileceği bir etkinlik.",
        "Yerel tiyatro topluluğunun sahne alacağı bir gösteri."
    };

        foreach (var interestArea in userInterestAreas)
        {
            var eventNamesForCategory = etkinlikAdlariDict.GetValueOrDefault(interestArea.KategoriId);
            if (eventNamesForCategory == null) continue;

            for (int i = 0; i < 5; i++)
            {

                var yeniEtkinlik = new Etkinlik
                {
                    EtkinlikAdi = faker.PickRandom(eventNamesForCategory),
                    Aciklama = faker.PickRandom(etkinlikAciklamalari),
                    Tarih = randomDateInDecember, // Rastgele belirlenen tarih
                    Saat = randomTime, // Rastgele belirlenen saat
                    EtkinlikSuresi = TimeSpan.FromHours(faker.Random.Int(1, 6)),
                    Konum = user.Konum,
                    Lat = user.Lat,
                    Lng = user.Lng,
                    KategoriId = interestArea.KategoriId,
                    OnayDurumu = true,
                    EtkinlikResmi = faker.Image.PicsumUrl(),
                    UserId = user.Id
                };

                etkinlikler.Add(yeniEtkinlik);

            }
        }

        _context.Etkinlikler.AddRange(etkinlikler);
        await _context.SaveChangesAsync();

        foreach (var bildirim in bildirimler)
        {
            bildirim.EtkinlikId = etkinlikler.FirstOrDefault(e => e.EtkinlikAdi == bildirim.Mesaj.Split(' ')[0])?.EtkinlikId ?? 0;
        }

        _context.Bildirimler.AddRange(bildirimler);
        await _context.SaveChangesAsync();
    }


    public async Task AddParticipantForInterest(User user)
    {
        var faker = new Faker("tr");

        // Kullanıcının ilgi alanları için rastgele kategori ID'leri oluştur
        var ilgiAlanları = new List<IlgiAlanı>();
        for (int i = 0; i < 5; i++)
        {
            var kategoriId = faker.Random.Int(1, 15); // Örnek kategori ID'si
            ilgiAlanları.Add(new IlgiAlanı
            {
                KullanıcıId = user.Id,
                KategoriId = kategoriId
            });
        }

        _context.IlgiAlanları.AddRange(ilgiAlanları);
        await _context.SaveChangesAsync();

        // Kullanıcının ilgi alanına göre etkinlikleri al ve katılımcı olarak ekle
        var etkinlikler = await _context.Etkinlikler
            .Where(e => ilgiAlanları.Select(ia => ia.KategoriId).Contains(e.KategoriId))
            .OrderBy(e => Guid.NewGuid()) // Rastgele sırala
            .Take(5) // İlk 5 etkinliği al
            .ToListAsync();

        foreach (var etkinlik in etkinlikler)
        {
            var katilimci = new Katilimci
            {
                KullanıcıId = user.Id,
                EtkinlikID = etkinlik.EtkinlikId
            };

            _context.Katilimcis.Add(katilimci);
        }

        await _context.SaveChangesAsync();
    }
    public async Task<List<Puan>> GenerateFakePuan()
    {
        // Kullanıcıları veritabanından al
        var users = await _context.Users.ToListAsync(); // Kullanıcıları veritabanından al
        var fakePuans = new List<Puan>();

        // Kullanıcı listesi üzerinden döngü
        foreach (var user in users)
        {
            fakePuans.Add(new Puan
            {
                KullaniciID = user.Id,         // Kullanıcı ID'sini al
                PuanDegeri = 130, // Rastgele bir puan ata
                KazanilanTarih = DateTime.Now   // Şu anki tarihi ata
            });
        }

        // Puanları veritabanına kaydetmek isterseniz:
        await _context.Puanlar.AddRangeAsync(fakePuans);
        await _context.SaveChangesAsync();

        return fakePuans;
    }
    private readonly string[] _yorumMetinleri = new[]
{
    "Bu etkinlik gerçekten harikaydı! Herkese tavsiye ederim.",
    "Etkinliğin organizasyonu çok iyiydi, tebrikler!",
    "Katıldığım en güzel etkinliklerden biriydi.",
    "Etkinlikte daha fazla aktivite olabilirdi.",
    "Umarım bu etkinlik gelecek yıl da yapılır!",
    "Bu etkinlikte tanıştığım insanlar harikaydı!",
    "Ellerinize sağlık, çok eğlendik.",
    "Fikirler çok güzeldi ama daha iyi bir planlama yapılabilirdi.",
    "Güzel bir etkinlikti ama biraz daha ilgi çekici olabilirdi.",
    "Her şey mükemmeldi, tekrar bekleriz!"
};
    [HttpPost("CreateRandomCommentsForEvent")]
    public async Task<IActionResult> CreateRandomCommentsForEvent()
    {
        var faker = new Faker("tr");
        var yorumlar = new List<Mesaj>();
        var users = await _userManager.Users.ToListAsync(); // Tüm kullanıcıları al
        var etkinlikler = await _context.Etkinlikler.ToListAsync(); // Tüm etkinlikleri al

        if (etkinlikler.Count == 0 || users.Count == 0)
        {
            // Eğer etkinlik veya kullanıcı yoksa işlemi durdur
            return BadRequest("Kullanıcılar veya etkinlikler bulunamadı.");
        }
        foreach (var etkinlik in etkinlikler)
        {
            for (int i = 0; i < 3; i++) // Her etkinlik için 3 yorum
            {
                var randomUser = faker.PickRandom(users); // Rastgele bir kullanıcı seç
                var yeniYorum = new Mesaj
                {
                    GondericiID = randomUser.UserName, // Kullanıcı adını GondericiID'ye ata
                    EtkinlikId = etkinlik.EtkinlikId,
                    MesajMetni = faker.PickRandom(_yorumMetinleri), // Rastgele bir yorum metni seç
                    GonderimZamani = DateTime.Now
                };

                yorumlar.Add(yeniYorum);
            }
        }


        _context.Mesajlar.AddRange(yorumlar); // Yorumları veritabanına ekle
        await _context.SaveChangesAsync(); // Değişiklikleri kaydet

        return RedirectToAction("GenerateFakeData", "Fake"); // Yönlendirme
    }

    public async Task<IActionResult> GenerateFakeKatilimciData(int count = 5)
    {
        var faker = new Faker("tr");

        // Tüm kullanıcıları çek
        var users = await _context.Users.ToListAsync();

        foreach (var user in users)
        {
            // Kullanıcının ilgi alanlarını al
            var userInterestAreas = await _context.IlgiAlanları
                .Where(ia => ia.KullanıcıId == user.Id)
                .Select(ia => ia.KategoriId)
                .ToListAsync();

            if (!userInterestAreas.Any())
            {
                // Eğer kullanıcının ilgi alanı yoksa sonraki kullanıcıya geç
                continue;
            }

            // İlgi alanlarına göre uygun etkinlikleri bul
            var etkinlikler = await _context.Etkinlikler
                .Where(e => userInterestAreas.Contains(e.KategoriId)) // İlgi alanlarına uygun etkinlikler
                .OrderBy(e => Guid.NewGuid()) // Rastgele sırala
                .Take(count)                  // Kullanıcının katılacağı etkinlik sayısı
                .ToListAsync();

            var katilimcilar = new List<Katilimci>();

            foreach (var etkinlik in etkinlikler)
            {
                // Katılımcıyı oluştur
                var katilimci = new Katilimci
                {
                    KullanıcıId = user.Id,   // Kullanıcının ID'sini ata
                    EtkinlikID = etkinlik.EtkinlikId // Etkinliğin ID'sini ata
                };

                katilimcilar.Add(katilimci);
            }

            // Katılımcıları topluca veritabanına ekle
            _context.Katilimcis.AddRange(katilimcilar);
        }

        // Tüm değişiklikleri kaydet
        await _context.SaveChangesAsync();

        return RedirectToAction("GenerateFakeDataForm");
    }
    // Rastgele bildirim oluştur ve ekle
    public async Task<IActionResult> CreateNotificationsForAllUsers()
    {
        var userIds = _context.Users.Select(u => u.Id).ToList(); // Veritabanındaki tüm kullanıcıları getir
        var userEvents = _context.Etkinlikler
            .Where(e => userIds.Contains(e.UserId)) // Yalnızca kullanıcılar tarafından oluşturulan etkinlikler
            .ToList(); // Etkinlikler listesine alınır

        if (!userIds.Any() || !userEvents.Any())
            return BadRequest("Kullanıcılar veya etkinlikler bulunamadı. Bildirim oluşturulamadı.");

        var notifications = new List<Bildirim>();

        // Tüm kullanıcılar için bildirimler oluşturuyoruz
        foreach (var userId in userIds)
        {
            foreach (var eventId in userEvents.Where(e => e.UserId == userId)) // Kullanıcıya ait etkinlikleri al
            {
                var notification = new Bildirim
                {
                    KullanıcıId = userId, // Her kullanıcı için bildirim
                    EtkinlikId = eventId.EtkinlikId, // Kullanıcıya ait etkinlik ID'sini kullan
                    BildirimTarih = DateTime.Now, // Anlık tarih
                    Mesaj = $"Etkinlik #{eventId.EtkinlikId} için bildirim!", // Etkinlik ID'si ile bildirim mesajı
                    IsAdminNotification = false // Admin bildirimi değilse false
                };

                notifications.Add(notification); // Bildirimi listeye ekle
            }
        }

        await _context.Bildirimler.AddRangeAsync(notifications); // Bildirimleri veritabanına ekleyelim
        await _context.SaveChangesAsync(); // Değişiklikleri kaydediyoruz

        return Ok($"{notifications.Count} adet bildirim tüm kullanıcılara oluşturuldu ve eklendi.");
    }



}
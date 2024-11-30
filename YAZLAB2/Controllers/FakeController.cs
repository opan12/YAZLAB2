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
    public IActionResult GenerateFakeData()
    {
        return View();
    }

    // Action to handle form submission for generating fake data
    [HttpPost]
    public async Task<IActionResult> GenerateFakeData(int count = 10)
    {
        var faker = new Faker("tr");
        var fakeUsers = new List<User>();
        var cities = new[]
                 {
            "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya",
            "Adana", "Gaziantep", "Konya", "Mersin", "Kayseri",
            "Sakarya", "Trabzon", "Aydın", "Bodrum", "Eskişehir",
            "Kocaeli", "Denizli", "Sivas", "Aksaray", "Malatya",
            "Bitlis", "Rize"
        };

        for (int i = 0; i < count; i++)
        {
            var city = faker.PickRandom(cities);
            var bounds = GetCityBounds(city);

            var fakeUser = new User
            {
                UserName = faker.Internet.UserName(),
                Ad = faker.Name.FirstName(),
                Soyad = faker.Name.LastName(),
                Email = faker.Internet.Email(),
                Konum = city,
                TelefonNumarasi = faker.Phone.PhoneNumber(),
                DogumTarihi = faker.Date.Past(30, DateTime.Now.AddYears(-18)),
                Cinsiyet = faker.PickRandom(new[] { "Erkek", "Kadın" }),
                ProfilFoto = faker.Image.PicsumUrl(),
                Lat = Math.Round(faker.Random.Double(bounds.minLat, bounds.maxLat), 6),
                Lng = Math.Round(faker.Random.Double(bounds.minLng, bounds.maxLng), 6)
            };

            fakeUsers.Add(fakeUser);
        }

        // Ensure "User" role exists
        var roleExist = await _roleManager.RoleExistsAsync("User");
        if (!roleExist)
        {
            var role = new IdentityRole("User");
            await _roleManager.CreateAsync(role);
        }

        // Create users and assign them to "User" role
        foreach (var user in fakeUsers)
        {
            var result = await _userManager.CreateAsync(user, "Test1234!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await CreateRandomInterestAreasForUser(user); // Create interest areas for each user
                await CreateRandomEventsForUser(user); // Create events for each user
            }
            else
            {
                return BadRequest($"Kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        return RedirectToAction("GenerateFakeData", "Fake");
    }

    private (double minLat, double maxLat, double minLng, double maxLng) GetCityBounds(string city)
    {
        return city switch
        {
            "İstanbul" => (40.5734, 41.3755, 28.1022, 29.4672),
            "Ankara" => (39.4901, 40.2255, 32.3054, 33.1003),
            "İzmir" => (38.2030, 38.6435, 26.7372, 27.5288),
            "Bursa" => (39.7850, 40.4636, 28.5500, 29.7010),
            "Antalya" => (36.5737, 37.3348, 30.0347, 31.2880),
            "Adana" => (37.0060, 37.1580, 35.1980, 35.4000),
            "Gaziantep" => (36.7160, 37.0360, 37.1040, 37.2630),
            "Konya" => (37.6572, 38.0530, 31.2563, 32.0240),
            "Mersin" => (36.6920, 37.0970, 34.1520, 34.4210),
            "Kayseri" => (38.5550, 38.9440, 35.4180, 36.0860),
            "Sakarya" => (40.4290, 40.8150, 30.3350, 30.7980),
            "Trabzon" => (39.6975, 40.2274, 39.5993, 40.0000),
            "Aydın" => (37.1550, 37.5520, 27.1570, 27.5110),
            "Bodrum" => (37.0170, 37.1300, 27.3060, 27.4140),
            "Eskişehir" => (39.7564, 39.8150, 30.3520, 30.4820),
            "Kocaeli" => (40.6880, 41.1430, 29.3310, 30.0550), // Kocaeli
            "Denizli" => (37.5150, 37.8320, 28.0580, 28.8070), // Denizli
            "Sivas" => (39.4633, 40.0147, 36.4790, 37.0780), // Sivas
            "Aksaray" => (38.2440, 38.5250, 33.2330, 34.0450), // Aksaray
            "Malatya" => (38.1960, 38.5520, 38.1570, 38.3620), // Malatya
            "Bitlis" => (38.3731, 38.5921, 42.0470, 42.4550), // Bitlis
            "Rize" => (40.0421, 41.1404, 40.0250, 41.0000), // Rize
            _ => (0, 0, 0, 0) // Default case
        };

    }

    private async Task CreateRandomInterestAreasForUser(User user)
    {
        var faker = new Faker("tr");
        var ilgiAlanları = new List<IlgiAlanı>();

        // Kategori ID'lerini rastgele seçmek için bir örnek (1-10 arasında)
        for (int i = 0; i < 5; i++)
        {
            var kategoriId = faker.Random.Int(1, 10); // Kategorilerinizin ID'leri 1-10 arasında olduğunu varsayıyoruz
            ilgiAlanları.Add(new IlgiAlanı
            {
                KullanıcıId = user.Id,
                KategoriId = kategoriId
            });
        }

        await _context.IlgiAlanları.AddRangeAsync(ilgiAlanları);
        await _context.SaveChangesAsync();
    }

    private async Task CreateRandomEventsForUser(User user)
    {
        var faker = new Faker("tr");
        var etkinlikler = new List<Etkinlik>();

        // Kullanıcının ilgi alanlarını al
        var userInterestAreas = await _context.IlgiAlanları
            .Where(ia => ia.KullanıcıId == user.Id)
            .ToListAsync();

        if (!userInterestAreas.Any())
        {
            // Eğer ilgi alanı yoksa, etkinlik oluşturma işlemini sonlandır
            return;
        }
        var etkinlikAdlari = new[]
         {
            "Yaz Festivali",
            "Sanat Sergisi",
            "Teknoloji Konferansı",
            "Müzik Dinletisi",
            "Kitap Fuarı",
            "Gastronomi Günü",
            "Çocuk Şenliği",
            "Spor Turnuvası",
            "Film Gösterimi",
            "Tiyatro Gösterisi"
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

        // Her ilgi alanı için etkinlik oluştur
        foreach (var interestArea in userInterestAreas)
        {
            for (int i = 0; i < 5; i++)
            {
                etkinlikler.Add(new Etkinlik
                {
                    EtkinlikAdi = faker.PickRandom(etkinlikAdlari),
                    Aciklama = faker.PickRandom(etkinlikAciklamalari),
                    Tarih = faker.Date.Future(30),
                    EtkinlikSuresi = TimeSpan.FromHours(faker.Random.Int(1, 6)),
                    Konum = user.Konum,
                    Lat = user.Lat, // Kullanıcının lat değeri
                    Lng = user.Lng, // Kullanıcının lng değeri
                    KategoriId = interestArea.KategoriId, // İlgi alanından kategori ID'sini kullan
                    OnayDurumu = true,
                    EtkinlikResmi = faker.Image.PicsumUrl(),
                    UserId = user.Id
                });
            }
        }

        _context.Etkinlikler.AddRange(etkinlikler);
        await _context.SaveChangesAsync();
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
    private async Task CreateRandomCommentsForEvent(Etkinlik etkinlik)
    {
        var faker = new Faker("tr");
        var yorumlar = new List<Mesaj>();
        var users = await _userManager.Users.ToListAsync(); // Tüm kullanıcıları al

        for (int i = 0; i < 3; i++) // Her etkinlik için 3 yorum
        {
            if (users.Count > 0)
            {
                var randomUser = faker.PickRandom(users);
                var yeniYorum = new Mesaj
                {
                    GondericiID = randomUser.Id,
                    EtkinlikId = etkinlik.EtkinlikId,
                    MesajMetni = faker.PickRandom(_yorumMetinleri), // Rastgele bir yorum metni seç
                    GonderimZamani = DateTime.Now
                };

                yorumlar.Add(yeniYorum);
            }
        }

        _context.Mesajlar.AddRange(yorumlar);
        await _context.SaveChangesAsync();
    }
    private async Task AddParticipantForInterest(User user)
    {
        var faker = new Faker("tr");

        // Kullanıcının ilgi alanları için rastgele kategori ID'leri oluştur
        var ilgiAlanları = new List<IlgiAlanı>();
        for (int i = 0; i < 5; i++)
        {
            var kategoriId = faker.Random.Int(1, 10); // Örnek kategori ID'si
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
    public List<Puan> GenerateFakePuan(List<User> users)
    {
        var fakePuans = new List<Puan>();

        // Kullanıcı listesi üzerinden döngü
        foreach (var user in users)
        {
            fakePuans.Add(new Puan
            {
                KullaniciID = user.Id, // Kullanıcı ID'sini al
                PuanDegeri = 130,      // Her kullanıcıya 130 puan ata
                KazanilanTarih = DateTime.Now // Şu anki tarihi ata
            });
        }

        return fakePuans;
    }

public async Task<IActionResult> GenerateFakeKatilimciData(int count = 10)
    {
        var faker = new Faker("tr");
        var katilimcilar = new List<Katilimci>();

        // Tüm kullanıcıları al
        var users = await _context.Users.ToListAsync(); // User tablosu

        foreach (var user in users)
        {
            // Kullanıcının ilgi alanlarını al
            var ilgiAlanlari = await _context.IlgiAlanları
                                              .Where(ia => ia.KullanıcıId == user.Id)
                                              .Select(ia => ia.KategoriId)
                                              .ToListAsync();

            // İlgi alanlarına göre etkinlikleri bul
            var etkinlikler = await _context.Etkinlikler
                                             .Where(e => ilgiAlanlari.Contains(e.KategoriId))
                                             .ToListAsync();

            // Her kullanıcı için etkinlikleri rastgele seç
            foreach (var etkinlik in etkinlikler)
            {
                katilimcilar.Add(new Katilimci
                {
                    KullanıcıId = user.Id, // Kullanıcının ID'sini al
                    EtkinlikID = etkinlik.EtkinlikId // Etkinliğin ID'sini al
                });
            }
        }

        // Katılımcıları veritabanına ekle
        await _context.Katilimcis.AddRangeAsync(katilimcilar);
        await _context.SaveChangesAsync();

        return RedirectToAction("GenerateFakeData", "Fake"); // Yeniden aynı sayfaya yönlendir
    }
}



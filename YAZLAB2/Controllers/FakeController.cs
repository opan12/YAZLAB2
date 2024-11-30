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

    public async Task<IActionResult> GenerateFakeData(int count = 10)
    {
        var faker = new Faker("tr");
        var fakeUsers = new List<User>();

        // İlçe bilgileri
        var districts = new List<(string District, double Lat, double Lng)>
{
    // İstanbul
    ("Kadıköy", 40.9939, 29.1043),
    
    // ...
};

        // Seçilen ilçeleri belirleyin (örnek)
        var selectedDistricts = new List<string>
{
     "Kadıköy",


    // Diğer ilçeleri de buraya ekleyebilirsiniz
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
                    Konum = district, // Sadece ilçe
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

        // Kullanıcı oluşturma ve rol ekleme işlemleri
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
        new Kategori { KategoriAdi = "Eğitim" }
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

        // Kullanıcının ilgi alanlarını al
        var userInterestAreas = await _context.IlgiAlanları
            .Where(ia => ia.KullanıcıId == user.Id)
            .ToListAsync();

        if (!userInterestAreas.Any())
        {
            // Eğer ilgi alanı yoksa, etkinlik oluşturma işlemini sonlandır
            return;
        }

        // Kategori ID'lerine göre etkinlik adlarını eşle
        var etkinlikAdlariDict = new Dictionary<int, string[]>
    {
        { 1, new[] { "Yaz Festivali", "Çocuk Şenliği" } }, // Örnek kategori ID'leri
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

        // Her ilgi alanı için etkinlik oluştur
        foreach (var interestArea in userInterestAreas)
        {
            var eventNamesForCategory = etkinlikAdlariDict.GetValueOrDefault(interestArea.KategoriId);
            if (eventNamesForCategory == null) continue; // Eğer kategori ID'sine uygun etkinlik adı yoksa devam et

           
                for (int i = 0; i < 5; i++)
                {
                    var yeniEtkinlik = new Etkinlik
                    {
                        EtkinlikAdi = faker.PickRandom(eventNamesForCategory), // Kategoriye özel etkinlik adı seç
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
                    };

                    etkinlikler.Add(yeniEtkinlik);

                    // Her etkinlik için yorum oluştur
                    await CreateRandomCommentsForEvent(yeniEtkinlik);
                }
            }

            _context.Etkinlikler.AddRange(etkinlikler);
        await _context.SaveChangesAsync();

    }

    public async Task AddParticipantForInterest(User user)
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
    public async Task<IActionResult> CreateRandomCommentsForEvent(Etkinlik etkinlik)
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
        return RedirectToAction("GenerateFakeData", "Fake"); // Yeniden aynı sayfaya yönlendir


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

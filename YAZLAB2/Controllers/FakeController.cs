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
    ("Üsküdar", 41.0245, 29.0250),
    ("Beşiktaş", 41.0438, 29.0027),
    ("Beyoğlu", 41.0329, 28.9770),
    ("Maltepe", 40.9834, 29.1143),
    ("Esenyurt", 41.0382, 28.6093),
    // Ankara
    ("Çankaya", 39.9334, 32.8597),
    ("Keçiören", 39.9932, 32.8636),
    ("Mamak", 39.9985, 32.8306),
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
    // Adana
    ("Seyhan", 37.0018, 35.3213),
    ("Yüreğir", 37.0082, 35.2691),
    ("Çukurova", 37.0224, 35.3244),
    // Kayseri
    ("Kocasinan", 38.7539, 35.4660),
    ("Melikgazi", 38.7331, 35.4821),
    ("Talas", 38.6910, 35.4507),
    // Gaziantep
    ("Şahinbey", 37.0670, 37.3764),
    ("Oğuzeli", 37.0842, 37.2172),
    ("Nizip", 37.1337, 37.0128),
    // Samsun
    ("İlkadım", 41.2865, 36.3355),
    ("Atakum", 41.3255, 36.2964),
    ("Canik", 41.3095, 36.2828),
    // Trabzon
    ("Ortahisar", 41.0027, 39.7166),
    ("Akçaabat", 40.6063, 39.5404),
    ("Sürmene", 40.8606, 39.6600),
    // Diyarbakır
    ("Sur", 37.9172, 40.2184),
    ("Yenişehir", 37.9140, 40.2260),
    ("Bağlar", 37.9087, 40.2311),
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
    // Diğer İllere Ait İlçeler
    ("Çanakkale Merkez", 40.1553, 26.4065),
    ("Bodrum", 37.0337, 27.4292),
    ("Marmaris", 36.8531, 28.2766),
    ("Fethiye", 36.6206, 29.1249),
    ("Kuşadası", 37.8625, 27.2671),
    // ...
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
    "Mamak",
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
    // Adana
    "Seyhan",
    "Yüreğir",
    "Çukurova",
    // Kayseri
    "Kocasinan",
    "Melikgazi",
    "Talas",
    // Gaziantep
    "Şahinbey",
    "Oğuzeli",
    "Nizip",
    // Samsun
    "İlkadım",
    "Atakum",
    "Canik",
    // Trabzon
    "Ortahisar",
    "Akçaabat",
    "Sürmene",
    // Diyarbakır
    "Sur",
    "Yenişehir",
    "Bağlar",
    // Konya
    "Selçuklu",
    "Meram",
    "Karatay",
    // Eskişehir
    "Tepebaşı",
    "Odunpazarı",
    // Kocaeli
    "İzmit",
    "Başiskele",
    "Gölcük",
    // Diğer İllere Ait İlçeler
    "Çanakkale Merkez",
    "Bodrum",
    "Marmaris",
    "Fethiye",
    "Kuşadası",
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
                etkinlikler.Add(new Etkinlik
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
                });
            }
        }

        _context.Etkinlikler.AddRange(etkinlikler);
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

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
[Route("Fake")]

public class FakeDataController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public FakeDataController(UserManager<User> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
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

        for (int i = 0; i < count; i++)
        {
            var fakeUser = new User
            {
                UserName = faker.Internet.UserName(),
                Ad = faker.Name.FirstName(),
                Soyad = faker.Name.LastName(),
                Email = faker.Internet.Email(),
                Konum = faker.Address.City(),
                TelefonNumarasi = faker.Phone.PhoneNumber(),
                DogumTarihi = faker.Date.Past(30, DateTime.Now.AddYears(-18)),
                Cinsiyet = faker.PickRandom(new[] { "Erkek", "Kadın" }),
                ProfilFoto = faker.Image.PicsumUrl()
            };

            fakeUsers.Add(fakeUser);
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
            }
            else
            {
                return BadRequest($"Kullanıcı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        return RedirectToAction("Index", "Home");  // Redirect to home after generation
    }

    // View to show categories
    public IActionResult AddCategoriview()
    {
        return View();
    }

    // Action to add categories to the database
    [HttpPost("AddCategories")]
    public async Task<IActionResult> AddCategories()
    {
        // Kategoriler
        var kategoriler = new List<Kategori>
    {
        new Kategori { KategoriAdi = "Teknoloji" },
        new Kategori { KategoriAdi = "Sanat" },
        new Kategori { KategoriAdi = "Müzik" },
        new Kategori { KategoriAdi = "Edebiyat" },
        new Kategori { KategoriAdi = "Bilim" },
        new Kategori { KategoriAdi = "Spor" }
    };

        // Kategorileri veritabanına ekle
        _context.Kategoris.AddRange(kategoriler);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Kategoriler başarıyla eklendi." });
    }


    // Action to assign random events to users
    [HttpPost]
    public async Task<IActionResult> AssignRandomEvents()
    {
        var users = await _context.Users.ToListAsync();
        if (users == null || !users.Any())
        {
            return NotFound("Hiç kullanıcı bulunamadı.");
        }

        var faker = new Faker("tr");
        var random = new Random();
        var etkinlikler = new List<Etkinlik>();
        var etkinlikShehirler = new Dictionary<string, List<string>>
        {
            // Define your etkinlikShehirler dictionary as you did previously
        };

        // Create events and assign them to users
        foreach (var etkinlikAdi in etkinlikShehirler.Keys)
        {
            var etkinlik = new Etkinlik
            {
                EtkinlikAdi = etkinlikAdi,
                Aciklama = faker.Lorem.Sentence(),
                Tarih = faker.Date.Future(1, DateTime.Now),
                EtkinlikSuresi = new TimeSpan(faker.Random.Number(1, 5), 0, 0),
                Konum = faker.PickRandom(etkinlikShehirler[etkinlikAdi]),
                KategoriId = faker.Random.Number(1, 3),
                OnayDurumu = faker.Random.Bool(),
            };

            etkinlikler.Add(etkinlik);
        }

        foreach (var user in users)
        {
            var kullaniciKonum = user.Konum;
            var uygunEtkinlikler = etkinlikler.Where(e => e.Konum == kullaniciKonum).ToList();

            if (uygunEtkinlikler.Any())
            {
                var randomEvent = uygunEtkinlikler[random.Next(uygunEtkinlikler.Count)];
                var etkinlik = new Etkinlik
                {
                    EtkinlikAdi = randomEvent.EtkinlikAdi,
                    Aciklama = randomEvent.Aciklama,
                    Tarih = randomEvent.Tarih,
                    EtkinlikSuresi = randomEvent.EtkinlikSuresi,
                    Konum = randomEvent.Konum,
                    KategoriId = randomEvent.KategoriId,
                    UserId = user.Id,
                    OnayDurumu = randomEvent.OnayDurumu
                };

                await _context.Etkinlikler.AddAsync(etkinlik);
            }
        }

        await _context.SaveChangesAsync();

        return View("AssignEventsSuccess"); // Redirect to a success view
    }
}

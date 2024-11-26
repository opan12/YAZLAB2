using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using YAZLAB2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using YAZLAB2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly EtkinlikOnerisiServisi _etkinlikOnerisiServisi;


    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, EtkinlikOnerisiServisi etkinlikOnerisiServisi
)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _etkinlikOnerisiServisi = etkinlikOnerisiServisi;

    }
    // GET: Register
    [HttpGet]
    public IActionResult Register()
    {
        // Kategorileri veritabanından çekiyoruz
        var kategoriler = _context.Kategoris.ToList();

        // Kategorileri SelectListItem formatına dönüştürüyoruz
        var kategoriList = kategoriler.Select(k => new SelectListItem
        {
            Value = k.KategoriId.ToString(),
            Text = k.KategoriAdi
        }).ToList();

        // ViewModel'i oluşturup, Kategoriler listesini set ediyoruz
        var model = new UserRegisterModel
        {
            Kategoriler = kategoriList
        };

        return View(model);
    }

    // Controller action to get the list of events
    [HttpGet("Etkinlikler")]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Etkinlikler.ToListAsync();  // Get the list of events from the database
        return Ok(events);  // Return the list as JSON
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserRegisterModel model)
    {
            // Yeni kullanıcı nesnesi oluşturuyoruz
            var user = new User
            {
                UserName = model.UserName,
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                TelefonNumarasi = model.TelefonNumarasi,
                Konum = model.Konum,
                DogumTarihi = model.DogumTarihi,
                Cinsiyet = model.Cinsiyet,
                ProfilFoto = model.ProfilFoto
            };

            // Kullanıcıyı veritabanına kaydediyoruz
            var result = await _userManager.CreateAsync(user, model.Şifre);

            if (result.Succeeded)
            {
                // Yeni kullanıcıya 'User' rolü ekliyoruz
                await _userManager.AddToRoleAsync(user, "User");

                // Seçilen ilgi alanlarını kontrol ediyoruz
                if (model.IlgiAlanlari != null && model.IlgiAlanlari.Any())
                {
                    // Veritabanında daha önce eklenmiş olan ilgi alanlarını alıyoruz
                    var mevcutIlgiAlanlari = await _context.IlgiAlanları
                        .Where(ia => ia.KullanıcıId == user.Id)
                        .ToListAsync();

                    // Yeni ilgi alanlarını oluşturuyoruz
                    var yeniIlgiAlanlari = model.IlgiAlanlari
                        .Where(kategoriId => !mevcutIlgiAlanlari.Any(ia => ia.KategoriId == kategoriId))
                        .Select(kategoriId => new IlgiAlanı
                        {
                            KullanıcıId = user.Id, // Kullanıcıyı ilişkilendiriyoruz
                            KategoriId = kategoriId // KategoriId'yi alıyoruz
                        })
                        .ToList();

                    // Yeni ilgi alanlarını veritabanına ekliyoruz
                    if (yeniIlgiAlanlari.Any())
                    {
                        await _context.IlgiAlanları.AddRangeAsync(yeniIlgiAlanlari);
                        await _context.SaveChangesAsync();
                    }
                }

                // Başarıyla işlemi tamamladıktan sonra giriş sayfasına yönlendiriyoruz
                return RedirectToAction("Login");
            }

            // Eğer işlem başarısızsa, model hatalarını ekliyoruz
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        

        // Model hatalıysa, aynı sayfayı tekrar döndürüyoruz
        return View(model);
    }

    // Fetch event suggestions for the user
    public async Task<IActionResult> UserHubArea()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Kullanıcı girişi gerekli.");
        }

        // Fetch event suggestions for the user
        var oneriListesi = _etkinlikOnerisiServisi.OneriGetir(user.Id);

        if (oneriListesi == null || !oneriListesi.Any())
        {
            TempData["Message"] = "Hiçbir etkinlik önerisi bulunamadı.";
            return View(); // No events to display
        }

        return View(oneriListesi); // Pass the event list to the View
    }

    // Kullanıcı Giriş Sayfası (View Gönderimi)
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
   
    // Kullanıcı Giriş İşlemi
    [HttpPost]
    public async Task<IActionResult> Login(UserLoginModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.Username, model.Şifre, false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["SuccessMessage"] = "Hoşgeldin Admin";
                return Redirect("/Admin/AdminHubArea");
            }
            else if (await _userManager.IsInRoleAsync(user, "User"))
            {
                TempData["SuccessMessage"] = "Hoşgeldin";
                return RedirectToAction("UserHubArea", "User", new { Username = model.Username });
            }


          
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
        }

        return View(model);
    }

  
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

      
        var model = new UserProfileViewModel
        {
            Ad = user.Ad,
            Soyad = user.Soyad,
            Email = user.Email,
            Konum = user.Konum,
            DogumTarihi = user.DogumTarihi,
            TelefonNumarasi = user.TelefonNumarasi,
            ProfilFoto = user.ProfilFoto,
        };

        return View(model);
    }
}
public class UserProfileViewModel
{
    public string Ad { get; set; }
    public string Soyad { get; set; }
    public string Email { get; set; }
    public string TelefonNumarasi { get; set; }
    public DateTime DogumTarihi { get; set; }
    public string ProfilFoto { get; set; }
    public string Konum { get; set; }
    public List<string> IlgiAlanlari { get; set; }
}
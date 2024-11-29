using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using YAZLAB2.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using YAZLAB2.Service;


public class UserController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly EtkinlikOnerisiServisi _etkinlikOnerisiServisi;
    private readonly EmailService _emailService;


    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, EtkinlikOnerisiServisi etkinlikOnerisiServisi,EmailService emailService
)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _etkinlikOnerisiServisi = etkinlikOnerisiServisi;
        _emailService = emailService;

    }
    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UpdateUser()
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
            TelefonNumarasi = user.TelefonNumarasi,
            Konum = user.Konum,
            DogumTarihi = user.DogumTarihi,
            Cinsiyet = user.Cinsiyet,
            ProfilFoto = user.ProfilFoto,
            UserName = user.UserName
        };

        return View(model);
    }
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UpdateUser(UserProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        user.Ad = model.Ad;
        user.Soyad = model.Soyad;
        user.Email = model.Email;
        user.TelefonNumarasi = model.TelefonNumarasi;
        user.Konum = model.Konum;
        user.DogumTarihi = model.DogumTarihi;
        user.Cinsiyet = model.Cinsiyet;
        user.ProfilFoto = model.ProfilFoto;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";

            var updatedModel = new UserProfileViewModel
            {
                Ad = user.Ad,
                Soyad = user.Soyad,
                Email = user.Email,
                TelefonNumarasi = user.TelefonNumarasi,
                Konum = user.Konum,
                DogumTarihi = user.DogumTarihi,
                Cinsiyet = user.Cinsiyet,
                ProfilFoto = user.ProfilFoto,
                UserName = user.UserName
            };

            return View("Profile", updatedModel);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        var kategoriler = _context.Kategoris.ToList();

        var kategoriList = kategoriler.Select(k => new SelectListItem
        {
            Value = k.KategoriId.ToString(),
            Text = k.KategoriAdi
        }).ToList();

        var model = new UserRegisterModel
        {
            Kategoriler = kategoriList
        };
        Random random = new Random();
        int randomNumber = random.Next(100000, 1000000);
        model.Code = randomNumber;

        return View(model);
    }
    [HttpPost]
    public IActionResult SendEmail(string email, string code)
    {

        _emailService.SendConfirmationEmail(email, code);

        return Json(new { success = true });
    }

    [HttpGet("Etkinlikler")]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Etkinlikler.ToListAsync(); 
        return Ok(events);  
    }
    private string GenerateConfirmationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // 6 haneli rastgele sayı
    }
    [HttpPost]
    public async Task<IActionResult> Register(UserRegisterModel model)
    {
     
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

        var result = await _userManager.CreateAsync(user, model.Şifre);
       
        
        if (result.Succeeded)
        {

            await _userManager.AddToRoleAsync(user, "User");

            if (model.IlgiAlanlari != null && model.IlgiAlanlari.Any())
            {
                var ilgiAlanlarıListesi = model.IlgiAlanlari
                    .Select(kategoriId => new IlgiAlanı
                    {
                        KullanıcıId = user.Id, 
                        KategoriId = kategoriId 
                    })
                    .ToList();

                await _context.IlgiAlanları.AddRangeAsync(ilgiAlanlarıListesi);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }
    
    public async Task<IActionResult> UserHubArea()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Kullanıcı girişi gerekli.");
        }

        var oneriListesi = await _etkinlikOnerisiServisi.OneriGetir(user.Id);

        if (oneriListesi == null || !oneriListesi.Any())
        {
            TempData["Message"] = "Hiçbir etkinlik önerisi bulunamadı.";
            return View(); 
        }

        return View(oneriListesi); 
    }
   
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
   
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

        // Fetch user's interests (if any)
        /*var ilgiAlanlari = await _context.IlgiAlanları
            .Where(ia => ia.KullanıcıId == user.Id)
            .Include(ia => ia.Kategori)
            .Select(ia => ia.Kategori.KategoriAdi)
            .ToListAsync();
        */
        var model = new YAZLAB2.Models.UserProfileViewModel
        {
            Ad = user.Ad,
            Soyad = user.Soyad,
            Email = user.Email,
            TelefonNumarasi = user.TelefonNumarasi,
            Konum = user.Konum,
            DogumTarihi = user.DogumTarihi,
            Cinsiyet = user.Cinsiyet,
            //IlgiAlanlari = ilgiAlanlari,
            Lat = user.Lat, // Kullanıcı Latitude bilgisi
            Lng = user.Lng,  // Kullanıcı Longitude bilgisi
            ProfilFoto = user.ProfilFoto,
            UserName = user.UserName
        };
        // Kullanıcının konumuna yakın etkinlikleri al
        var nearbyEvents = await GetNearbyEventsAsync(user.Lat, user.Lng);
        model.NearbyEvents = nearbyEvents;

        return View(model);
    }
    public async Task<List<NearbyEventViewModel>> GetNearbyEventsAsync(double userLat, double userLng)
    {
        var nearbyEvents = new List<NearbyEventViewModel>();

        // Etkinlikleri al
        var events = await _context.Etkinlikler.ToListAsync();

        foreach (var eventItem in events)
        {
            double eventLat = eventItem.Lat; // Etkinlik koordinatları
            double eventLng = eventItem.Lng;

            // Mesafe hesaplama (Haversine Formula)
            double distance = CalculateDistance(userLat, userLng, eventLat, eventLng);

            // 10 km içinde olan etkinlikleri al
            if (distance <= 100)
            {
                nearbyEvents.Add(new NearbyEventViewModel
                {
                    EventName = eventItem.EtkinlikAdi,  // Etkinlik adı
                    Location = eventItem.Konum,         // Etkinlik yeri
                    Distance = distance                  // Mesafe
                });
            }
        }

        return nearbyEvents;
    }

    private double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        var R = 6371; // Dünya yarıçapı (km cinsinden)
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c; // Mesafe (km cinsinden)
        return distance;
    }

    private double ToRadians(double degree)
    {
        return degree * Math.PI / 180;
    }

    [HttpGet]
    [Authorize]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Email != model.Email)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz işlem. E-posta adresi mevcut kullanıcıyla eşleşmiyor.");
                return View(model);
            }

            TempData["Message"] = "Şifre sıfırlama talebi başarılı. Yeni şifrenizi belirlemek için devam edin.";
            return RedirectToAction("ResetPassword");
        }

        return View(model);
    }

    [HttpGet]
    [Authorize]
    public IActionResult ResetPassword()
    {
        return View(new ResetPasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Geçersiz işlem. Kullanıcı bulunamadı.");
                return View(model);
            }

            var result = await _userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                var addPasswordResult = await _userManager.AddPasswordAsync(user, model.Şifre);
                if (addPasswordResult.Succeeded)
                {
                    TempData["Message"] = "Şifreniz başarıyla güncellendi.";
                    return RedirectToAction("Profile");
                }
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View(model);
    }

}


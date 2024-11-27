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


    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context, EtkinlikOnerisiServisi etkinlikOnerisiServisi
)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _etkinlikOnerisiServisi = etkinlikOnerisiServisi;

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

        return View(model);
    }

    [HttpGet("Etkinlikler")]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Etkinlikler.ToListAsync(); 
        return Ok(events);  
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
            ProfilFoto = user.ProfilFoto,
            UserName = user.UserName
        };

        return View(model); 
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


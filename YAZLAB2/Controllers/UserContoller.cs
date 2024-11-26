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

    public UserController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }
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

                return RedirectToAction("Login");
            
         
        }

        // Return view if the model is not valid
        return View(model);
    }


[HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> UserHubArea(string Username)
    {
        if (string.IsNullOrEmpty(Username))
        {
            return BadRequest("Username parametresi eksik.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == Username);
        if (user == null)
        {
            return NotFound($"Kullanıcı {Username} bulunamadı.");
        }

        return View(user); // Kullanıcıyı view'e gönder
    }



    // Kullanıcı Giriş Sayfası (View Gönderimi)
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminHubArea(string username)
    {

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

        if (user == null)
        {
            return NotFound("Kullanıcı bulunamadı.");
        }

        // Admin kullanıcı ise Admin Hub'a yönlendir
        if (user.UserName == "admin")
        {
            return RedirectToAction("AdminDashboard", "Admin");
        }

        return View(user); // Eğer admin değilse normal kullanıcı view'ını göster
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
                return RedirectToAction("AdminHubArea", "User");
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
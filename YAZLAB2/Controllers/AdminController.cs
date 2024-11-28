using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using YAZLAB2.Models;
using YAZLAB2.Data;


namespace YAZLAB2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [AllowAnonymous]
        public async Task<JsonResult> AdminRegister()
        {
            var Appuser = new User
            {
                UserName = "admin",
                Ad = "",
                Soyad = "",
                Email = "",
                TelefonNumarasi = "",
                Konum = "",
                DogumTarihi = DateTime.Now,
                Cinsiyet = "",
                ProfilFoto = "",




            };


            IdentityResult result = await _userManager.CreateAsync(Appuser, "AWDj#BBGAq2q2C");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(Appuser, "Admin");
                return Json("Kayıt Başarılı");
            }

            return Json("Kayıt Başarısız");
        }
        [Authorize(Roles = "Admin")]

        public IActionResult AdminHubArea()
        {
            return View("AdminHubArea"); // Doğru görünüm adı
        }

        /*
        // Kullanıcıları Listele
        public async Task<IActionResult> TumKullanicilar()
        {
            //var users = await _userManager.Users.ToListAsync();
            var users = await _userManager.Users
                                  .Where(user => user.UserName != "admin")
                                  .ToListAsync();
            if (users == null || !users.Any())
            {
                ViewBag.Message = "Hiç kullanıcı bulunamadı.";
                return View();
            }
            return View(users);
        }
        */
        public async Task<IActionResult> TumKullanicilar()
        {
            var users = await _userManager.Users
                                          .Where(user => user.UserName != "admin")
                                          .ToListAsync();
            if (users == null || !users.Any())
            {
                ViewBag.Message = "Hiç kullanıcı bulunamadı.";
                return View();
            }
            return View(users);
        }

        // Kullanıcı Düzenle
        [HttpGet]
        public async Task<IActionResult> KullaniciDuzenle(string id)
        {
            var kullanici = await _userManager.FindByIdAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }
            return View(kullanici);
        }

        [HttpPost]
        public async Task<IActionResult> KullaniciDuzenle(User kullanici)
        {
            if (!ModelState.IsValid)
            {
                return View(kullanici);
            }

            var mevcutKullanici = await _userManager.FindByIdAsync(kullanici.Id);
            if (mevcutKullanici == null)
            {
                return NotFound();
            }

            mevcutKullanici.UserName = kullanici.UserName;
            mevcutKullanici.Email = kullanici.Email;
            mevcutKullanici.Ad = kullanici.Ad;
            mevcutKullanici.Soyad = kullanici.Soyad;
            mevcutKullanici.TelefonNumarasi = kullanici.TelefonNumarasi;
            mevcutKullanici.Konum = kullanici.Konum;
            mevcutKullanici.DogumTarihi = kullanici.DogumTarihi;
            mevcutKullanici.Cinsiyet = kullanici.Cinsiyet;
            mevcutKullanici.ProfilFoto = kullanici.ProfilFoto;

            var result = await _userManager.UpdateAsync(mevcutKullanici);
            if (result.Succeeded)
            {
                TempData["Message"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction("TumKullanicilar");
            }

            TempData["ErrorMessage"] = "Kullanıcı güncellenemedi.";
            return View(kullanici);
        }


        // Kullanıcı Sil
        [HttpPost]
        public async Task<IActionResult> KullaniciSil(string id)
        {
            var kullanici = await _userManager.FindByIdAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(kullanici);
            if (result.Succeeded)
            {
                TempData["Message"] = "Kullanıcı başarıyla silindi.";
                return RedirectToAction("TumKullanicilar");
            }

            TempData["ErrorMessage"] = "Kullanıcı silinemedi.";
            return RedirectToAction("TumKullanicilar");
        }


        // Etkinlikleri Listele
        public async Task<IActionResult> TumEtkinlikler()
        {
            var etkinlikler = await _context.Etkinlikler.ToListAsync();
        

           
            return View(etkinlikler);
        }

        // Kategori Ekle
        [HttpGet]
        public IActionResult KategoriEkle()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> KategoriEkle(Kategori kategori)
        {
            if (!ModelState.IsValid)
            {
                return View(kategori);
            }

            var mevcutKategori = await _context.Kategoris
                .FirstOrDefaultAsync(k => k.KategoriAdi == kategori.KategoriAdi);

            if (mevcutKategori != null)
            {
                ModelState.AddModelError("", "Bu kategori zaten mevcut.");
                return View(kategori);
            }

            _context.Kategoris.Add(kategori);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Kategori başarıyla eklendi.";
            return RedirectToAction("TumEtkinlikler");
        }

        // Etkinlik Güncelle
        [HttpGet]
        public async Task<IActionResult> EtkinlikDuzenle(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }
            return View(etkinlik);
        }

        [HttpPost]
        public async Task<IActionResult> EtkinlikDuzenle(Etkinlik etkinlik)
        {
            if (!ModelState.IsValid)
            {
                return View(etkinlik);
            }

            _context.Entry(etkinlik).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Etkinlik başarıyla güncellendi.";
            return RedirectToAction("TumEtkinlikler");
        }
        
        // Etkinlik Sil
        [HttpPost]
        public async Task<IActionResult> EtkinlikSil(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            _context.Etkinlikler.Remove(etkinlik);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Etkinlik başarıyla silindi.";
            return RedirectToAction("TumEtkinlikler");
        }

        // Etkinlik Onayla
        [HttpPost]
        public async Task<IActionResult> EtkinlikOnayla(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            etkinlik.OnayDurumu = true;
            _context.Entry(etkinlik).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Etkinlik onaylandı.";
            return RedirectToAction("TumEtkinlikler");
        }

        // Etkinlik Reddet
        [HttpPost]
        public async Task<IActionResult> EtkinlikReddet(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            etkinlik.OnayDurumu = false;
            _context.Entry(etkinlik).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Etkinlik reddedildi.";
            return RedirectToAction("TumEtkinlikler");
        }
    }
}

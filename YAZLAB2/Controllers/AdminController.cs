using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using YAZLAB2.Models;
using YAZLAB2.Data;
using YAZLAB2.Services;


namespace YAZLAB2.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly BildirimService _bildirimService;


        public AdminController(ApplicationDbContext context, UserManager<User> userManager, BildirimService bildirimService)
        {
            _context = context;
            _userManager = userManager;
            _bildirimService = bildirimService;


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


            IdentityResult result = await _userManager.CreateAsync(Appuser, "");
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

        public async Task<IActionResult> Mesaj(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            var mesajlar = await _context.Mesajlar
                .Where(m => m.EtkinlikId == id)
                .OrderByDescending(m => m.GonderimZamani)
                .ToListAsync();

            ViewData["Mesajlar"] = mesajlar; // Mesajları view'a gönder

            return View(etkinlik);
        }
        [HttpPost]
        public async Task<IActionResult> MesajSil(int mesajId)
        {
            var mesaj = await _context.Mesajlar.FindAsync(mesajId);
            if (mesaj != null)
            {
                _context.Mesajlar.Remove(mesaj);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Detay", new { id = mesaj.EtkinlikId });
        }

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
        public async Task<IActionResult> KullaniciDetay(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var katilimciEtkinlikler = await _context.Katilimcis
                .Where(k => k.KullanıcıId == user.Id)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

            var duzenledigiEtkinlikler = await _context.Etkinlikler
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            var detayModel = new KullaniciEtkinlikViewModel
            {
                User = user,
                KatildigiEtkinlikler = await _context.Etkinlikler
                    .Where(e => katilimciEtkinlikler.Contains(e.EtkinlikId))
                    .ToListAsync(),
                DuzenledigiEtkinlikler = duzenledigiEtkinlikler
            };

            return View(detayModel);
        }

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

        public async Task<IActionResult> Detay(int id)
        {
            var etkinlik = await (from e in _context.Etkinlikler
                                  join k in _context.Kategoris on e.KategoriId equals k.KategoriId
                                  join u in _context.Users on e.UserId equals u.Id
                                  where e.EtkinlikId == id
                                  select new EtkinlikDetayViewModel
                                  {
                                      EtkinlikId = e.EtkinlikId,
                                      EtkinlikAdi = e.EtkinlikAdi,
                                      Aciklama = e.Aciklama,
                                      Tarih = e.Tarih,
                                      Saat = e.Saat,
                                      EtkinlikSuresi = e.EtkinlikSuresi,
                                      Konum = e.Konum,
                                      KategoriAdi = k.KategoriAdi,
                                      KullaniciAdi = u.UserName,
                                      EtkinlikResmi = e.EtkinlikResmi,
                                      OnayDurumu = e.OnayDurumu
                                  }).FirstOrDefaultAsync();

            if (etkinlik == null)
            {
                return NotFound();
            }

            return View(etkinlik);
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
            ViewData["Kategoriler"] = _context.Kategoris.ToList();

            return View(etkinlik);
        }

        // Etkinlik Güncelle
        [HttpPost]
        public async Task<IActionResult> EtkinlikDuzenle(Etkinlik etkinlik)
        {
            if (!ModelState.IsValid)
            {
                return View(etkinlik);
            }

            _context.Entry(etkinlik).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var admin = await _userManager.GetUserAsync(User); // İşlemi yapan admin
            var hedefKullanici = await _context.Users.FindAsync(etkinlik.UserId); // Etkinlik sahibi kullanıcı
            if (admin != null && hedefKullanici != null)
            {
                await _bildirimService.AddBildirimAsync(
                    hedefKullanici.Id,
                    etkinlik.EtkinlikId,
                    $"{admin.UserName} tarafından etkinliğiniz güncellendi.",
                    isAdminNotification: true // Burada isAdminNotification parametresi ekleniyor
                );
            }

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

            var admin = await _userManager.GetUserAsync(User); // İşlemi yapan admin
            var hedefKullanici = await _context.Users.FindAsync(etkinlik.UserId); // Etkinlik sahibi kullanıcı
            if (admin != null && hedefKullanici != null)
            {
                await _bildirimService.AddBildirimAsync(
                    hedefKullanici.Id,
                    etkinlik.EtkinlikId,
                    $"{admin.UserName} tarafından etkinliğiniz silindi.",
                    isAdminNotification: true // Burada isAdminNotification parametresi ekleniyor
                );
            }

            TempData["Message"] = "Etkinlik başarıyla silindi.";
            return RedirectToAction("TumEtkinlikler");
        }
    
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

            // Bildirim oluştur
            var admin = await _userManager.GetUserAsync(User);
            var hedefKullanici = await _context.Users.FindAsync(etkinlik.UserId); // Etkinlik sahibi kullanıcı
            if (admin != null && hedefKullanici != null)
            {
                await _bildirimService.AddBildirimAsync(
                    hedefKullanici.Id,
                    id,
                    $"{admin.UserName} tarafından etkinliğiniz onaylandı.",
                    isAdminNotification: true // Burada isAdminNotification parametresi ekleniyor
                );
            }

            TempData["Message"] = "Etkinlik onaylandı.";
            return RedirectToAction("TumEtkinlikler");
        }
        public async Task<IActionResult> TumPuanlar()
        {
            var puanlar = await _context.Puanlar
               .Join(_context.Users, // Join işlemi
                     puan => puan.KullaniciID, // Puanlar tablosundaki KullaniciID'yi User tablosundaki Id ile eşleştir
                     user => user.Id,
                     (puan, user) => new // Yeni bir anonim tip oluşturuyoruz
                     {
                         PuanId = puan.PuanId,
                         KullaniciID = puan.KullaniciID,
                         UserName = user.UserName, // Kullanıcı adı
                         PuanDegeri = puan.PuanDegeri,
                         KazanilanTarih = puan.KazanilanTarih
                     })
               .ToListAsync(); // Listeye dönüştür

            return View(puanlar); // View'a gönder
        }

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

            // Bildirim oluştur
            var admin = await _userManager.GetUserAsync(User);
            var hedefKullanici = await _context.Users.FindAsync(etkinlik.UserId); // Etkinlik sahibi kullanıcı
            if (admin != null && hedefKullanici != null)
            {
                await _bildirimService.AddBildirimAsync(
                    hedefKullanici.Id,
                    id,
                    $"{admin.UserName} tarafından etkinliğiniz reddedildi.",
                    isAdminNotification: true // Burada isAdminNotification parametresi ekleniyor
                );
            }

            TempData["Message"] = "Etkinlik reddedildi.";
            return RedirectToAction("TumEtkinlikler");
        }
    }
}

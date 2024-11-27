using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Yazlab__2.Service;
using YAZLAB2.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using YAZLAB2.Services;



namespace Yazlab__2.Controllers
{
    [Authorize]
    public class EtkinlikController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly EtkinlikService _etkinlikService;
        private readonly BildirimService _bildirimService;


        public EtkinlikController(ApplicationDbContext context, UserManager<User> userManager, EtkinlikService etkinlikService, BildirimService bildirimService)
        {
            _context = context;
            _userManager = userManager;
            _etkinlikService = etkinlikService;
            _bildirimService = bildirimService;

        }

        // Kullanıcının etkinliklerini listele
        public async Task<IActionResult> Index()
        {

            var user = await _userManager.GetUserAsync(User);
            var etkinlikler = await _context.Etkinlikler
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            return View(etkinlikler);
        }


        // Etkinlik Detayı
        public async Task<IActionResult> Details(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }

            return View(etkinlik);
        }
        [HttpPost]
        public async Task<IActionResult> Katil(int etkinlikId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var kullaniciEtkinlikleri = await _context.Katilimcis
                .Where(k => k.KullanıcıId == user.Id)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

            var kullaniciIlgiAlani = await _context.IlgiAlanları
                .Where(k => k.KullanıcıId == user.Id)
                .Select(k => k.KategoriId)
                .ToListAsync();

            if (kullaniciEtkinlikleri.Contains(etkinlikId))
            {
                TempData["Error"] = "Bu etkinliğe zaten katıldınız.";
                return RedirectToAction("Details", new { id = etkinlikId });
            }

            var katilimci = new Katilimci
            {
                KullanıcıId = user.Id,
                EtkinlikID = etkinlikId,
            };

            _context.Katilimcis.Add(katilimci);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Etkinliğe başarıyla katıldınız.";  // Başarı mesajı
            return RedirectToAction("Details", new { id = etkinlikId });  // Aynı sayfaya geri döner
        }



        // Yeni Etkinlik Oluşturma (Get)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Kategorileri veritabanından al
            var kategoriler = await _context.Kategoris.ToListAsync();

            // Yeni bir Etkinlik modelini oluştur
            var model = new Etkinlik();

            // Etkinlik modeli için kategorileri ViewData ile göndereceğiz
            ViewData["Kategoriler"] = kategoriler;

            return View(model);
        }


        // Yeni Etkinlik Oluşturma (Post)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Etkinlik yeniEtkinlik)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Kullanıcı ID'sini ve onay durumunu ayarlıyoruz.
            yeniEtkinlik.UserId = user.Id;
            yeniEtkinlik.OnayDurumu = false;

            // Etkinliği oluşturma işlemi
            var result = await _etkinlikService.CreateEtkinlik(yeniEtkinlik);
            if (!result)
            {
                // Eğer etkinlik oluşturulamazsa, kategorilerle birlikte tekrar formu gösteriyoruz.
                var kategoriler = await _context.Kategoris.ToListAsync();
                ViewData["Kategoriler"] = kategoriler;
                ModelState.AddModelError(string.Empty, "Etkinlik tarihi ve saati çakışıyor.");
                return View(yeniEtkinlik);
            }
            await _bildirimService.AddBildirimAsync(user.Id, yeniEtkinlik.EtkinlikId); // Kullanıcıya bildirim

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var etkinlik = await _context.Etkinlikler
                .FirstOrDefaultAsync(e => e.EtkinlikId == id && e.UserId == user.Id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            return View(etkinlik);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Etkinlik updatedEvent)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            var etkinlik = await _context.Etkinlikler
                .FirstOrDefaultAsync(e => e.EtkinlikId == updatedEvent.EtkinlikId && e.UserId == user.Id);

            

            if (etkinlik == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(updatedEvent);
            }


            etkinlik.EtkinlikAdi = updatedEvent.EtkinlikAdi;
            etkinlik.Aciklama = updatedEvent.Aciklama;
            etkinlik.Tarih = updatedEvent.Tarih;
            etkinlik.Konum = updatedEvent.Konum;

            try
            {
                _context.Etkinlikler.Update(etkinlik);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Veritabanı hatası: " + ex.Message);
                return View(updatedEvent);
            }

            return RedirectToAction("Details", new { id = etkinlik.EtkinlikId });
        }




   
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var etkinlik = await _context.Etkinlikler
                .FirstOrDefaultAsync(e => e.EtkinlikId == id && e.UserId == user.Id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            _context.Etkinlikler.Remove(etkinlik);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Etkinlik başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }



       
    }
}

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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userCreatedEvents = await _context.Etkinlikler
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            var userParticipatedEvents = await _context.Katilimcis
                .Where(k => k.KullanıcıId == user.Id)
                .Select(k => k.EtkinlikID)
                .ToListAsync();

            var participatedEvents = await _context.Etkinlikler
                .Where(e => userParticipatedEvents.Contains(e.EtkinlikId))
                .ToListAsync();

            ViewData["ParticipatedEvents"] = participatedEvents;

            return View(userCreatedEvents); 
        }

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

            TempData["Success"] = "Etkinliğe başarıyla katıldınız."; 
            return RedirectToAction("Details", new { id = etkinlikId });  
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var kategoriler = await _context.Kategoris.ToListAsync();
            var model = new Etkinlik();

            ViewData["Kategoriler"] = kategoriler;

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Etkinlik yeniEtkinlik)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            yeniEtkinlik.UserId = user.Id;
            yeniEtkinlik.OnayDurumu = false;

            var result = await _etkinlikService.CreateEtkinlik(yeniEtkinlik);
            if (!result)
            {
                var kategoriler = await _context.Kategoris.ToListAsync();
                ViewData["Kategoriler"] = kategoriler;
                ModelState.AddModelError(string.Empty, "Etkinlik tarihi ve saati çakışıyor.");
                return View(yeniEtkinlik);
            }
            await _bildirimService.AddBildirimAsync(user.Id, yeniEtkinlik.EtkinlikId); 

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
            ViewData["Kategoriler"] = _context.Kategoris.ToList(); // Kategorileri yükleyin

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

       

            etkinlik.EtkinlikAdi = updatedEvent.EtkinlikAdi;
            etkinlik.Aciklama = updatedEvent.Aciklama;
            etkinlik.Tarih = updatedEvent.Tarih;
            etkinlik.Saat = updatedEvent.Saat;
            etkinlik.Konum = updatedEvent.Konum;
            etkinlik.KategoriId = updatedEvent.KategoriId;  
            etkinlik.EtkinlikSuresi = updatedEvent.EtkinlikSuresi;
            etkinlik.EtkinlikResmi = updatedEvent.EtkinlikResmi;

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

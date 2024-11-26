using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Yazlab__2.Service;
using YAZLAB2.Data;



namespace Yazlab__2.Controllers
{
    [Authorize]
    public class EtkinlikController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly EtkinlikService _etkinlikService;

        public EtkinlikController(ApplicationDbContext context, UserManager<User> userManager, EtkinlikService etkinlikService)
        {
            _context = context;
            _userManager = userManager;
            _etkinlikService = etkinlikService;
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

        // Yeni Etkinlik Oluşturma (Get)
        [HttpGet]
        public IActionResult Create()
        {
            // Yeni bir Etkinlik modelini görünüm için oluşturun
            var model = new Etkinlik();
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


            yeniEtkinlik.UserId = user.Id;
            yeniEtkinlik.OnayDurumu = false;

            var result = await _etkinlikService.CreateEtkinlik(yeniEtkinlik);
            if (!result)
            {
                ModelState.AddModelError(string.Empty, "Etkinlik tarihi ve saati çakışıyor.");
                return View(yeniEtkinlik);
            }

            return RedirectToAction(nameof(Index));
        }

        // Etkinlik Güncelleme (Get)
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

        // Etkinlik Güncelleme (Post)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Etkinlik updatedEvent)
        {
            var user = await _userManager.GetUserAsync(User);
            var etkinlik = await _context.Etkinlikler
                .FirstOrDefaultAsync(e => e.EtkinlikId == updatedEvent.EtkinlikId && e.UserId == user.Id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedEvent);
            }

            etkinlik.EtkinlikAdi = updatedEvent.EtkinlikAdi;
            etkinlik.Aciklama = updatedEvent.Aciklama;
            etkinlik.Tarih = updatedEvent.Tarih;
            etkinlik.Konum = updatedEvent.Konum;

            _context.Entry(etkinlik).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Etkinlik Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            return RedirectToAction(nameof(Index));
        }
    }
}

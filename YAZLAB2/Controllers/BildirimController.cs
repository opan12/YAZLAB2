using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using YAZLAB2.Data;
using YAZLAB2.Models;
using YAZLAB2.Services;

namespace YAZLAB2.Controllers
{
    [Authorize] // Tüm kullanıcılar için oturum açma gerektirir
    public class BildirimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly BildirimService _bildirimService;


        public BildirimController(ApplicationDbContext context, BildirimService bildirimService, UserManager<User> userManager)
        {
            _context = context;
            _bildirimService = bildirimService;
            _userManager = userManager; // Bu satır eksikse NullReferenceException alabilirsiniz.

        }

        // Kullanıcının bildirimlerini listele
        [Authorize(Roles = "User")]
        public async Task<IActionResult> KullaniciBildirimler()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var bildirimler = await _bildirimService.GetKullaniciBildirimlerAsync(user.Id);
            return View(bildirimler);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminBildirimler()
        {
            var bildirimler = await _bildirimService.GetAdminBildirimlerAsync();
            return View(bildirimler);
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Giriş yapan kullanıcının ID'sini al
            var bildirimler = await _context.Bildirimler
                .Where(b => b.KullanıcıId == userId)
                .OrderByDescending(b => b.BildirimTarih)
                .ToListAsync();

            return View(bildirimler);
        }

        // Belirli bir bildirimi sil
        [HttpPost]
        public async Task<IActionResult> Sil(int id)
        {
            var bildirim = await _context.Bildirimler.FindAsync(id);
            if (bildirim != null)
            {
                _context.Bildirimler.Remove(bildirim);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Bildirim başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Bildirim bulunamadı.";
            }

            return RedirectToAction("Index");
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Yazlab__2.Service;
using YAZLAB2.Models;
using System.Threading.Tasks;
using YAZLAB2.Data;

namespace Yazlab__2.Controllers
{
    public class PuanController : Controller
    {
        private readonly PuanHesaplayiciService _puanHesaplayiciService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public PuanController(PuanHesaplayiciService puanHesaplayiciService, UserManager<User> userManager, ApplicationDbContext contex)
        {
            _puanHesaplayiciService = puanHesaplayiciService;
            _userManager = userManager;
            _context = contex;
        }

        // Toplam Puan Görüntüleme
        public async Task<IActionResult> Index()
        {
            // Giriş yapan kullanıcıyı alıyoruz
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                TempData["Error"] = "Puan bilgilerini görüntülemek için giriş yapmanız gereklidir.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Kullanıcının ID'sini alıyoruz
                var userId = user.Id;

                // Toplam puanı hesaplıyoruz
                var toplamPuan = await _puanHesaplayiciService.HesaplaToplamPuan(userId);

                // Modeli View'e gönderiyoruz
                ViewBag.ToplamPuan = toplamPuan;
                return View(user);
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajını View'e gönderiyoruz
                TempData["Error"] = $"Bir hata oluştu: {ex.Message}";
                return View();
            }
        }


        private async Task KaydetPuan(string userId, int puanDegeri)
        {
            var yeniPuan = new Puan
            {
                KullaniciID = userId,
                PuanDegeri = puanDegeri,
                KazanilanTarih = DateTime.Now // Puanın kazanıldığı tarihi şu an olarak ayarlıyoruz
            };

            // Puanı veritabanına ekliyoruz
            await _context.Puanlar.AddAsync(yeniPuan);
            await _context.SaveChangesAsync(); // Değişiklikleri kaydediyoruz
        }
    }
}
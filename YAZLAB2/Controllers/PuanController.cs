using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Yazlab__2.Service;
using YAZLAB2.Models;
using System.Threading.Tasks;

namespace Yazlab__2.Controllers
{
    public class PuanController : Controller
    {
        private readonly PuanHesaplayiciService _puanHesaplayiciService;
        private readonly UserManager<User> _userManager;

        public PuanController(PuanHesaplayiciService puanHesaplayiciService, UserManager<User> userManager)
        {
            _puanHesaplayiciService = puanHesaplayiciService;
            _userManager = userManager;
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
    }
}

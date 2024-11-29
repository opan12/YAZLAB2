using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using YAZLAB2.Hubs; // SignalR'ı ekle

namespace YAZLAB2.Controllers
{
    public class MesajController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext; // SignalR Hub Context

        public MesajController(ApplicationDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext; // Hub context'i al
        }

        // Etkinlik bazında mesajları listeleme
        public async Task<IActionResult> EtkinlikMesajlari(int etkinlikId)
        {
            var mesajlar = await _context.Mesajlar
                .Where(m => m.EtkinlikId == etkinlikId)
                .OrderBy(m => m.GonderimZamani)
                .ToListAsync();

            // EtkinlikId'yi ViewData ile geçirme
            ViewData["EtkinlikId"] = etkinlikId;

            return View(mesajlar);
        }

        // Etkinliğe yeni mesaj ekleme
        [HttpPost]
        public async Task<IActionResult> EtkinlikMesajEkle(int etkinlikId, string mesajMetni)
        {
            if (string.IsNullOrEmpty(mesajMetni))
            {
                return BadRequest("Mesaj boş olamaz.");
            }

            var yeniMesaj = new Mesaj
            {
                GondericiID = User.Identity.Name,  // Kullanıcı adı veya ID
                AliciID = null,  // Eğer mesaj bireysel değilse, alıcı bilgisi null olabilir
                MesajMetni = mesajMetni,
                GonderimZamani = DateTime.Now,
                EtkinlikId = etkinlikId
            };

            _context.Mesajlar.Add(yeniMesaj);
            await _context.SaveChangesAsync();

            // Bildirim gönderme
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{User.Identity.Name} yeni bir mesaj gönderdi.");

            return RedirectToAction("EtkinlikMesajlari", new { etkinlikId = etkinlikId });  // Mesaj ekledikten sonra etkinlik mesajları sayfasına yönlendir
        }

        // Yeni mesaj ekleme
        // Mesaj gönderme işlemi
        [HttpPost]
        public async Task<IActionResult> MesajGonder(string aliciUsername, string mesajMetni)
        {
            if (string.IsNullOrEmpty(aliciUsername) || string.IsNullOrEmpty(mesajMetni))
            {
                return BadRequest("Geçersiz giriş.");
            }

            // Kullanıcı adı ile alıcı ID'sini bulma
            var alici = await _context.Users.FirstOrDefaultAsync(u => u.UserName == aliciUsername);
            if (alici == null)
            {
                return BadRequest("Alıcı bulunamadı.");
            }

            var yeniMesaj = new Mesaj
            {
                GondericiID = User.Identity.Name,  // Kullanıcı adı veya ID
                AliciID = alici.Id, // Alıcının ID'si
                MesajMetni = mesajMetni,
                GonderimZamani = DateTime.Now,
                EtkinlikId = 0 // İlgili etkinlik ID'si varsa eklenebilir
            };

            _context.Mesajlar.Add(yeniMesaj);
            await _context.SaveChangesAsync();

            // Bildirim gönderme
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"{User.Identity.Name} yeni bir mesaj gönderdi.");

            return RedirectToAction("Mesajlarim");  // Gönderim sonrası gelen mesajlar sayfasına yönlendir
        }

        [HttpGet]
        public async Task<IActionResult> Mesajlarim()
        {
            // Kullanıcı bilgilerini al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
            }

            // Kullanıcının ID'sini al
            var userId = user.Id;

            // Mesajları filtrele
            var mesajlar = await _context.Mesajlar
                .Where(m => m.AliciID == userId)
                .OrderByDescending(m => m.GonderimZamani)
                .ToListAsync();

            return View(mesajlar);
        }

        [HttpGet]
        public IActionResult MesajGonder()
        {
            return View(); // Mesaj gönderim formunu içeren görünümü döndür
        }

        [HttpGet]
        public async Task<IActionResult> GonderilenMesajlar()
        {
            var gondericiId = User.Identity.Name; // Gönderenin kullanıcı adı

            // Mesajları ve alıcının kullanıcı adını birleştirerek çekiyoruz
            var mesajlar = await _context.Mesajlar
                .Where(m => m.GondericiID == gondericiId)
                .Select(m => new
                {
                    Mesaj = m,
                    AliciUsername = _context.Users
                        .Where(u => u.Id == m.AliciID)
                        .Select(u => u.UserName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Modeli hazırlıyoruz
            var model = mesajlar.Select(m => new Mesaj
            {
                MesajMetni = m.Mesaj.MesajMetni,
                GonderimZamani = m.Mesaj.GonderimZamani,
                AliciID = m.AliciUsername // Kullanıcı adı burada atanıyor
            }).ToList();

            return View(model);
        }
    }
}

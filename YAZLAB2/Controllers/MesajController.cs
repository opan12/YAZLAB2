using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;

namespace YAZLAB2.Controllers
{
    public class MesajController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MesajController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Etkinlik bazında mesajları listeleme
        public async Task<IActionResult> EtkinlikMesajlari(int etkinlikId)
        {
            var mesajlar = await _context.Mesajlar
                .Where(m => m.EtkinlikId == etkinlikId)
                .OrderBy(m => m.GonderimZamani)
                .ToListAsync();

            return View(mesajlar);
        }

        // Yeni mesaj ekleme
        // Mesaj gönderme işlemi
        [HttpPost]
        public async Task<IActionResult> MesajGonder(string aliciId, string mesajMetni)
        {
            if (string.IsNullOrEmpty(aliciId) || string.IsNullOrEmpty(mesajMetni))
            {
                return BadRequest("Geçersiz giriş.");
            }

            var yeniMesaj = new Mesaj
            {
                GondericiID = User.Identity.Name,  // Kullanıcı adı veya ID
                AliciID = aliciId,
                MesajMetni = mesajMetni,
                GonderimZamani = DateTime.Now,
                EtkinlikId = 0 // İlgili etkinlik ID'si varsa eklenebilir
            };

            _context.Mesajlar.Add(yeniMesaj);
            await _context.SaveChangesAsync();

            return RedirectToAction("Mesajlarim");  // Gönderim sonrası gelen mesajlar sayfasına yönlendir
        }

        // Kullanıcıya ait tüm mesajları görüntüleme
        public async Task<IActionResult> Mesajlarim()
        {
            var aliciId = User.Identity.Name;  // Kullanıcı ID'si

            var mesajlar = await _context.Mesajlar
                .Where(m => m.AliciID == aliciId)
                .OrderByDescending(m => m.GonderimZamani)
                .ToListAsync();

            return View(mesajlar);
        }

        // Kullanıcıya gönderenlerin mesajlarını listeleme
        public async Task<IActionResult> GonderilenMesajlar()
        {
            var gondericiId = User.Identity.Name;  // Kullanıcı ID'si

            var mesajlar = await _context.Mesajlar
                .Where(m => m.GondericiID == gondericiId)
                .OrderByDescending(m => m.GonderimZamani)
                .ToListAsync();

            return View(mesajlar);
        }
    }
}

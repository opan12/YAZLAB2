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
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Newtonsoft.Json.Linq;


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
        [HttpGet]
        public async Task<IActionResult> Rota(int etkinlikId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var etkinlik = await _context.Etkinlikler.FindAsync(etkinlikId);
            if (etkinlik == null)
            {
                return NotFound("Etkinlik bulunamadı.");
            }

            // ViewData'ya veritabanından alınan koordinatları yerleştirin
            ViewData["UserLat"] = user.Lat;
            ViewData["UserLng"] = user.Lng;
            ViewData["EtkinlikLat"] = etkinlik.Lat;
            ViewData["EtkinlikLng"] = etkinlik.Lng;

            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["UserLat"] = user.Lat;
            ViewData["UserLng"] = user.Lng;
            ViewData["EtkinlikLat"] = etkinlik.Lat;
            ViewData["EtkinlikLng"] = etkinlik.Lng;

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

            var etkinlik = await _context.Etkinlikler.FindAsync(etkinlikId);
            if (etkinlik == null)
            {
                TempData["Error"] = "Etkinlik bulunamadı.";
                return RedirectToAction("Index"); // Geri dönmek için uygun bir sayfa
            }

            // Kullanıcının daha önce katıldığı etkinliklerin tarih ve saatlerini al
            var kullaniciEtkinlikleri = await _context.Katilimcis
                .Where(k => k.KullanıcıId == user.Id)
                .Join(_context.Etkinlikler,
                      katilimci => katilimci.EtkinlikID,
                      etkinlik => etkinlik.EtkinlikId,
                      (katilimci, etkinlik) => new
                      {
                          etkinlik.Tarih,
                          etkinlik.Saat,
                          etkinlik.Konum
                      })
                .ToListAsync();

            // Seçilen etkinliğin tarih ve saat bilgilerini al
            var etkinlikTarihi = etkinlik.Tarih;
            var etkinlikSaati = etkinlik.Saat;

            // Çakışma kontrolü
            var cakismaVarMi = kullaniciEtkinlikleri.Any(k =>
                k.Tarih == etkinlikTarihi &&
                k.Saat == etkinlikSaati &&
                k.Konum == etkinlik.Konum); // Aynı gün, saat ve konumda etkinlik varsa

            if (cakismaVarMi)
            {
                // Alternatif etkinlik öner
                var alternatifEtkinlikler = await _context.Etkinlikler
                    .Where(e => e.Tarih == etkinlikTarihi && e.Saat != etkinlikSaati && e.Konum == etkinlik.Konum && e.OnayDurumu == true)
                    .ToListAsync();

                if (alternatifEtkinlikler.Any())
                {
                    TempData["Warning"] = "Bu etkinliğe katılımınız çakışma yaratıyor. Aşağıdaki alternatif etkinlikleri değerlendirebilirsiniz.";
                    return View("AlternatifEtkinlikler", alternatifEtkinlikler); // Alternatif etkinlikler için yeni bir görünüm döndür
                }

                TempData["Error"] = "Bu etkinliğe katıldığınız için başka bir etkinlikte çakışma yaşanıyor.";
                return RedirectToAction("Details", new { id = etkinlikId });
            }

            // Katılım kaydını ekle
            var katilimci = new Katilimci
            {
                KullanıcıId = user.Id,
                EtkinlikID = etkinlikId,
            };

            _context.Katilimcis.Add(katilimci);
            await _context.SaveChangesAsync();

            // Belge oluşturma
            using var pdfStream = GenerateParticipationDocument(user, etkinlik); // Belgeyi oluştur

            // Başarı mesajı
            TempData["Success"] = "Etkinliğe başarıyla katıldınız.";

            return File(pdfStream.ToArray(), "application/pdf", "katilim_belgesi.pdf");
        }


        private MemoryStream GenerateParticipationDocument(User user, Etkinlik etkinlik)
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Belge içeriğini ekleyin
            document.Add(new Paragraph("Katılım Belgesi").SetFontSize(20));
            document.Add(new Paragraph($"Kullanıcı Adı: {user.UserName}"));
            document.Add(new Paragraph($"Etkinlik Adı: {etkinlik.EtkinlikAdi}"));
            document.Add(new Paragraph($"Tarih: {etkinlik.Tarih.ToShortDateString()}"));
            document.Add(new Paragraph($"Saat: {etkinlik.Saat}"));

            document.Close();

            return stream;
        }

        [HttpGet]
        private async Task<(double Latitude, double Longitude)> GetCoordinatesAsync(string location)
        {
            using (HttpClient client = new HttpClient())
            {
                string accessToken = "pk.eyJ1Ijoic2VseWlsIiwiYSI6ImNsdjUyN2d1ZTBkY28yamxidXRxYm1tNnUifQ.Uqy4MfIj3drA__4mvRldfw"; // Mapbox Access Token
                string url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{Uri.EscapeDataString(location)}.json?access_token={accessToken}";

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(json);

                    var coordinates = data["features"]?[0]?["geometry"]?["coordinates"];
                    if (coordinates != null)
                    {
                        double longitude = (double)coordinates[0];
                        double latitude = (double)coordinates[1];
                        return (latitude, longitude);
                    }
                }

                throw new Exception("Konum koordinatlara dönüştürülemedi.");
            }
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

            // Fetch coordinates for the location provided
            if (!string.IsNullOrWhiteSpace(yeniEtkinlik.Konum))
            {
                try
                {
                    var (latitude, longitude) = await GetCoordinatesAsync(yeniEtkinlik.Konum);
                    yeniEtkinlik.Lat = latitude;
                    yeniEtkinlik.Lng = longitude;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Konum koordinatları alınırken hata oluştu: " + ex.Message);
                    var kategoriler = await _context.Kategoris.ToListAsync();
                    ViewData["Kategoriler"] = kategoriler;
                    return View(yeniEtkinlik);
                }
            }

            var result = await _etkinlikService.CreateEtkinlik(yeniEtkinlik);
            if (!result)
            {
                var kategoriler = await _context.Kategoris.ToListAsync();
                ViewData["Kategoriler"] = kategoriler;
                ModelState.AddModelError(string.Empty, "Etkinlik tarihi ve saati çakışıyor.");
                return View(yeniEtkinlik);
            }
            await _bildirimService.AddBildirimAsync(user.Id, yeniEtkinlik.EtkinlikId, "Oluşturuldu", isAdminNotification: true);

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
        [ValidateAntiForgeryToken]
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

            // Konum koordinatlarını alma işlemi
            if (!string.IsNullOrWhiteSpace(updatedEvent.Konum))
            {
                try
                {
                    var (latitude, longitude) = await GetCoordinatesAsync(updatedEvent.Konum);
                    etkinlik.Lat = latitude;
                    etkinlik.Lng = longitude;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Konum koordinatları alınırken hata oluştu: " + ex.Message);
                    ViewData["Kategoriler"] = await _context.Kategoris.ToListAsync();
                    return View(updatedEvent);
                }
            }

            // Etkinliği güncelleme işlemi
            try
            {
                _context.Etkinlikler.Update(etkinlik);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Veritabanı hatası: " + ex.Message);
                ViewData["Kategoriler"] = await _context.Kategoris.ToListAsync();
                return View(updatedEvent);
            }

            // Başarılı güncelleme sonrası detay sayfasına yönlendirme
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

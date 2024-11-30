using Microsoft.AspNetCore.Mvc;
using YAZLAB2.Models;
using YAZLAB2.Service; // StatisticsService'in bulunduğu namespace
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using YAZLAB2.Data;
using Microsoft.EntityFrameworkCore;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
public class StatisticsController : Controller
{
    private readonly StatisticsService _statisticsService;
    private readonly ApplicationDbContext _context;


    public StatisticsController(StatisticsService statisticsService, ApplicationDbContext context)
    {
        _statisticsService = statisticsService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userCountByGender = await _statisticsService.GetUserCountByGender();
        var eventCountByCategory = await _statisticsService.GetEventCountByCategory();
        var userCountByAgeGroup = await _statisticsService.GetUserCountByAgeGroup();
        var userPoints = await _statisticsService.GetUserPoints(); // Fetch user points

        var model = new StatisticsViewModel
        {
            UserCountByGender = userCountByGender,
            EventCountByCategory = eventCountByCategory,
            UserCountByAgeGroup = userCountByAgeGroup,
            UserPoints = userPoints // Add user points to the model
        };

        return View(model);
    }
    public async Task<List<UserEventReport>> GetUserEventReport()
    {
        var userEventReport = await _context.Etkinlikler
            .GroupBy(e => e.UserId)
            .Select(g => new UserEventReport
            {
                KullaniciAdı = _context.Users.FirstOrDefault(u => u.Id == g.Key).UserName, // UserId yerine Username al
                OluşturulanEtkinlikSayisi = g.Count(),
                KategoriId = g.Select(e => e.KategoriId).FirstOrDefault(),
                KatıldığıEtkinlikSayisi = _context.Katilimcis.Count(k => k.KullanıcıId == g.Key)
            })
            .ToListAsync();

        return userEventReport;
    }

    public async Task GenerateUserEventReport()
    {
        var reports = await GetUserEventReport();

        // Dosya yolunu belirtin
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserEventReport.pdf");

        using (var writer = new PdfWriter(filePath))
        using (var pdf = new PdfDocument(writer))
        {
            var document = new Document(pdf);

            // Başlık için yazı tipi ayarlama
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var title = new Paragraph("Kullanıcı Etkinlik Raporu")
                .SetFont(font)
                .SetFontSize(20);
            document.Add(title);

            // Tablo oluştur
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1, 1 })).UseAllAvailableWidth();
            table.AddHeaderCell("Kullanıcı Adı");
            table.AddHeaderCell("Oluşturulan Etkinlik Sayısı");
            table.AddHeaderCell("Kategori ID");
            table.AddHeaderCell("Katıldığı Etkinlik Sayısı");

            // Rapor verilerini tabloya ekle
            foreach (var report in reports)
            {
                // Null kontrolleri
                string kullaniciAdi = report.KullaniciAdı ?? "Belirtilmedi";
                string olusturulanEtkinlikSayisi = report.OluşturulanEtkinlikSayisi.ToString();
                string kategoriId = report.KategoriId.ToString() ?? "N/A";
                string katildigiEtkinlikSayisi = report.KatıldığıEtkinlikSayisi.ToString();

                table.AddCell(kullaniciAdi);
                table.AddCell(olusturulanEtkinlikSayisi);
                table.AddCell(kategoriId);
                table.AddCell(katildigiEtkinlikSayisi);
            }

            // Tabloyu belgeye ekle
            document.Add(table);
            document.Close();
        }
    }


    public async Task<IActionResult> DownloadUserEventReport()
    {
        // Kullanıcı etkinlik raporunu oluştur
        await GenerateUserEventReport();

        // PDF dosyasının yolunu belirleyin
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserEventReport.pdf");

        // Dosya var mı kontrol et
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(); // Dosya bulunamazsa 404 döndür
        }

        // PDF dosyasını indirme olarak döndür
        return File(System.IO.File.ReadAllBytes(filePath), "application/pdf", "UserEventReport.pdf");
    }

}

using Microsoft.EntityFrameworkCore;
using YAZLAB2.Data;
using YAZLAB2.Models;

namespace YAZLAB2.Service
{
    public class StatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetUserCountByGender()
        {
            return await _context.Users
                .GroupBy(u => u.Cinsiyet)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Gender, g => g.Count);
        }

        public async Task<Dictionary<int, int>> GetEventCountByCategory()
        {
            return await _context.Etkinlikler
                .GroupBy(e => e.KategoriId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.CategoryId, g => g.Count); // Burada Key ve Count int olarak dönecek
        }

        // Yaş dağılımını almak için bir metod
        public async Task<Dictionary<int, int>> GetUserCountByAgeGroup()
        {
            return await _context.Users
                .Where(u => u.DogumTarihi.HasValue)
                .GroupBy(u => DateTime.Now.Year - u.DogumTarihi.Value.Year)
                .Select(g => new { Age = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Age, g => g.Count);
        }
        public async Task<Dictionary<string, int>> GetUserPoints()
        {
            return await _context.Users
                .Select(u => new
                {
                    UserName = u.UserName,
                    TotalPoints = _context.Puanlar.Where(p => p.KullaniciID == u.Id).Sum(p => p.PuanDegeri) // Assuming Puanlar contains a PuanDegeri property
                })
                .ToDictionaryAsync(u => u.UserName, u => u.TotalPoints);
        }
      
    }
}

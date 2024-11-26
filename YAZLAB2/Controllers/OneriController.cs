using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using YAZLAB2.Models;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Yazlab__2.Service;
using YAZLAB2.Data;
using Microsoft.EntityFrameworkCore;

namespace Yazlab__2.Controllers
{
   
    [Authorize]

    public class OneriController : ControllerBase
    {
        private readonly EtkinlikOnerisiServisi _etkinlikOnerisiServisi;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;


        // Injecting the EtkinlikOnerisiServisi service into the controller
        // Injecting required services into the controller
        public OneriController(
            EtkinlikOnerisiServisi etkinlikOnerisiServisi,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ApplicationDbContext context)
        {
            _etkinlikOnerisiServisi = etkinlikOnerisiServisi;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet("GetOneriler")]
        public async Task<IActionResult> GetOneriler()
        {
            // Kullanıcının kimliğini al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("Kullanıcı girişi gerekli.");
            }

            // Kullanıcının etkinlik önerilerini al
            var oneriListesi = _etkinlikOnerisiServisi.OneriGetir(user.Id);

            if (oneriListesi == null || !oneriListesi.Any())
            {
                return NotFound("No event suggestions found.");
            }

            return Ok(oneriListesi);
        }
        //[HttpGet("GetFilteredOneriler")]
        //public async Task<IActionResult> GetFilteredOneriler(string? konum = null, string? kategori = null)
        //{
        //    // Kullanıcının kimliğini al
        //    var user = await _userManager.GetUserAsync(User);
        //    if (user == null)
        //    {
        //        return Unauthorized("Kullanıcı girişi gerekli.");
        //    }

        //    // Kullanıcının tüm etkinlik önerilerini al
        //    var oneriListesi = _etkinlikOnerisiServisi.OneriGetir(user.Id);

        //    // Filtreleme
        //    if (!string.IsNullOrEmpty(konum))
        //    {
        //        // Konuma göre filtreleme
        //        oneriListesi = oneriListesi.Where(o => o.Konum == konum).ToList();
        //    }
        //    if (!string.IsNullOrEmpty(kategori))
        //    {
        //        if (int.TryParse(kategori, out int kategoriId))
        //        {
        //            // Kategoriye göre filtreleme
        //            oneriListesi = oneriListesi.Where(o => o.KategoriId == kategoriId).ToList();
        //        }
        //        else
        //        {
        //            // Geçersiz kategori girildiğinde yapılacak işlemler
        //            return BadRequest("Geçersiz kategori değeri.");
        //        }
        //    }

        //    if (!oneriListesi.Any())
        //    {
        //        return NotFound("No event suggestions found for the specified criteria.");
        //    }

        //    return Ok(oneriListesi);
        //}

    }
}

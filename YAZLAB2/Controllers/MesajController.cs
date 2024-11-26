namespace Yazlab__2.WebApi.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Yazlab__2.Core.Service;
    using System.Security.Claims;

    [Route("api/[controller]")]
    [ApiController]
    public class MesajController : ControllerBase
    {
        private readonly MesajServisi _mesajServisi;

        public MesajController(MesajServisi mesajServisi)
        {
            _mesajServisi = mesajServisi;
        }

        // Etkinliğe yorum ekleme
        [HttpPost("YorumEkle")]
        public async Task<IActionResult> YorumEkle([FromBody] YorumEkleModel model)
        {
            var GöndericiId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Kullanıcının ID'sini alıyoruz

            if (string.IsNullOrEmpty(GöndericiId) || string.IsNullOrEmpty(model.MesajMetni) || model.EtkinlikId == 0)
            {
                return BadRequest("Geçersiz giriş.");
            }

            var sonuc = await _mesajServisi.YorumEkle(GöndericiId, model.EtkinlikId, model.MesajMetni);

            if (sonuc)
            {
                return Ok("Yorum başarıyla eklendi.");
            }

            return BadRequest("Yorum eklenemedi.");
        }
        [HttpGet("KullaniciMesajlari")]
        public async Task<IActionResult> KullaniciMesajlari()
        {
            var GöndericiId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Kullanıcının ID'sini alıyoruz

            if (string.IsNullOrEmpty(GöndericiId))
            {
                return BadRequest("Kullanıcı kimliği bulunamadı.");
            }

            // Kullanıcıya ait tüm mesajları alıyoruz
            var mesajlar = await _mesajServisi.KullaniciMesajlari(GöndericiId);

            if (mesajlar == null || mesajlar.Count == 0)
            {
                return NotFound("Hiç mesaj bulunamadı.");
            }

            return Ok(mesajlar);
        }
        // Etkinlik yorumlarını getirme
        [HttpGet("EtkinlikYorumlari/{etkinlikId}")]
        public async Task<IActionResult> EtkinlikYorumlari(int etkinlikId)
        {
            var yorumlar = await _mesajServisi.EtkinlikYorumlari(etkinlikId);

            if (yorumlar == null || yorumlar.Count == 0)
            {
                return NotFound("Hiç yorum bulunamadı.");
            }

            return Ok(yorumlar);
        }

        [HttpPost("MesajGonder")]
        public async Task<IActionResult> MesajGonder([FromBody] MesajGonderModel model)
        {
            var GöndericiId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Kullanıcının ID'sini alıyoruz

            if (string.IsNullOrEmpty(GöndericiId) || string.IsNullOrEmpty(model.MesajMetni))
            {
                return BadRequest("Geçersiz giriş.");
            }

            var sonuc = await _mesajServisi.MesajGonder(GöndericiId, model.AlıcıId, model.MesajMetni);

            if (sonuc)
            {
                return Ok("Mesaj başarıyla gönderildi.");
            }

            return BadRequest("Mesaj gönderilemedi.");
        }

        // Yorum ekleme model
        public class YorumEkleModel
        {
            public int EtkinlikId { get; set; }
            public string MesajMetni { get; set; }
        }

        // Mesaj gönderme model
        public class MesajGonderModel
        {
            public string AlıcıId { get; set; }
            public string MesajMetni { get; set; }
        }
    }
}

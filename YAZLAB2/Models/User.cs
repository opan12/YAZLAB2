using Microsoft.AspNetCore.Identity;

namespace YAZLAB2.Models
{
    public class User : IdentityUser
    {
        public string Konum { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public string Cinsiyet { get; set; }
        //public List<string> IlgiAlanlari { get; set; }
        public string TelefonNumarasi { get; set; }
        public string ProfilFoto { get; set; }  // Fotoğrafı byte dizisi olarak saklıyoruz
    }
}
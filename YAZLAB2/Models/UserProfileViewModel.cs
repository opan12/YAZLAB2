using Microsoft.AspNetCore.Mvc.Rendering;

namespace YAZLAB2.Models
{
    public class UserProfileViewModel
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Email { get; set; }
        public string TelefonNumarasi { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public string ProfilFoto { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Konum { get; set; }
        public List<int> IlgiAlanlari { get; set; } // Bu özellik ilgi alanlarını tutar
        public string Cinsiyet { get; set; }
        public string UserName { get; set; }
        public List<NearbyEventViewModel> NearbyEvents { get; set; } // Kullanıcının yakınındaki etkinlikler

        // Kategoriler özelliği eklendi
        public List<SelectListItem> Kategoriler { get; set; } // Kullanıcının ilgi alanı kategorileri
    }

    public class NearbyEventViewModel
    {
        public string EventName { get; set; }  // Etkinlik adı
        public string Location { get; set; }   // Etkinlik yeri
        public double Distance { get; set; }   // Mesafe
    }

    // Kategori modelini burada tanımlayın
   
}

namespace YAZLAB2.Models
{
  
        public class KullaniciEtkinlikViewModel
        {
            public User User { get; set; } // User bilgilerini tutacak özellik
            public List<Etkinlik> KatildigiEtkinlikler { get; set; } // Kullanıcının katıldığı etkinlikler
            public List<Etkinlik> DuzenledigiEtkinlikler { get; set; } // Kullanıcının düzenlediği etkinlikler
        }


    }

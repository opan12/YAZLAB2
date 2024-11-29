namespace YAZLAB2.Models
{
    public class UserEventReport
    {
        public string KullaniciAdı { get; set; } // Kullanıcı ID yerine kullanıcı adı
        public int OluşturulanEtkinlikSayisi { get; set; }
        public int KategoriId { get; set; }
        public int KatıldığıEtkinlikSayisi { get; set; }
    }

}

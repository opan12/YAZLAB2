namespace YAZLAB2.Models
{
    public class UserRegisterModel
    {
        public string UserName { get; set; }
        public string Şifre { get; set; }
        public string Konum { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string Cinsiyet { get; set; }
        public string TelefonNumarasi { get; set; }
        public string ProfilFoto { get; set; } // Base64 veya URL olarak saklanabilir
        public List<int> IlgiAlanlari { get; set; }  // KategoriId listesi
        public string Email { get; set; }


    }
}

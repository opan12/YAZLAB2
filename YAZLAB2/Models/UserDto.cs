namespace YAZLAB2.Models
{
    public class UserDto
    {
        public string Id { get; set; }            // Kullanıcı ID'si
        public string Ad { get; set; }             // Kullanıcının adı
        public string Soyad { get; set; }          // Kullanıcının soyadı
        public string Konum { get; set; }          // Kullanıcının konumu
        public string IlgiAlanlari { get; set; }   // Kullanıcının ilgi alanları
        public DateTime? DogumTarihi { get; set; } // Kullanıcının doğum tarihi
        public string Cinsiyet { get; set; }       // Kullanıcının cinsiyeti
        public string TelefonNumarasi { get; set; } // Kullanıcının telefon numarası
        public string ProfilFoto { get; set; }     // Kullanıcının profil fotoğrafı
        public string Email { get; set; }          // Kullanıcının e-posta adresi
    }

}

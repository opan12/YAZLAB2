namespace YAZLAB2.Models
{

    public class EtkinlikDetayViewModel
    {
        public int EtkinlikId { get; set; }
        public string EtkinlikAdi { get; set; }
        public string Aciklama { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public TimeSpan EtkinlikSuresi { get; set; }
        public string Konum { get; set; }
        public string KategoriAdi { get; set; }
        public string KullaniciAdi { get; set; }
        public string EtkinlikResmi { get; set; }
        public bool OnayDurumu { get; set; }
    }

}



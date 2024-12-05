using System.ComponentModel.DataAnnotations;

namespace YAZLAB2.Models
{
    public class EtkinlikDto
    {
        public int EtkinlikId { get; set; }

        public string EtkinlikAdi { get; set; }
        public string Aciklama { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan Saat { get; set; }
        public TimeSpan EtkinlikSuresi { get; set; }
        public string Konum { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Kategori { get; set; }
        public string KatilimDurumu { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace YAZLAB2.Models
{
    public class Kategori
    {
        [Key]
        public int KategoriId { get; set; }

        [Required]
        public string KategoriAdi { get; set; }

        public int? EtkinlikId { get; set; }
    }
}

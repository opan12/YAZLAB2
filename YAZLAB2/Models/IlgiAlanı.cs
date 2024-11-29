using System.ComponentModel.DataAnnotations;

namespace YAZLAB2.Models
{
    public class IlgiAlanı
    {
        [Key]
        public int Id { get; set; }
        public string KullanıcıId { get; set; }
        public int KategoriId { get; set; }


    }
}

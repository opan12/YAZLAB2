using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Kategori
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

    public int KategoriId { get; set; }
    public string KategoriAdi { get; set; }

    // Etkinlik ID'si, sadece etkinlik oluşturulurken set edilecek.
    public int? EtkinlikId { get; set; } // Nullable yapıyoruz, böylece boş olabilir
}

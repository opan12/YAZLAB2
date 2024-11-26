namespace YAZLAB2.Models;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Etkinlik
{
    [Key]
    public int EtkinlikId { get; set; }

    [Required]
    public string EtkinlikAdi { get; set; }
    public string Aciklama { get; set; }
    public DateTime Tarih { get; set; }
    public TimeSpan Saat { get; set; }
    public TimeSpan EtkinlikSuresi { get; set; }
    public string Konum { get; set; }

    // KategoriId dış anahtar olarak belirtildi.
    //[ForeignKey("Kategori")] // Bu, Kategori ile ilişkiyi belirtir.
    public int KategoriId { get; set; }  // Bu, dış anahtar (foreign key) olacak.

    public string UserId { get; set; }
    public bool OnayDurumu { get; set; }
}

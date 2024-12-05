namespace YAZLAB2.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Mesaj
    {
        [Key]
        public int MesajID { get; set; }
        public string GondericiID { get; set; }
        public int EtkinlikId { get; set; }
        public string? AliciID { get; set; }
        [Required]

        public string MesajMetni { get; set; }
        public DateTime GonderimZamani { get; set; }
        public int? ParentMesajId { get; set; }


    }
}

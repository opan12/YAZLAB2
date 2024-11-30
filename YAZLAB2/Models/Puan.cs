namespace YAZLAB2.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Puan
    {
        [Key]
        public int PuanId { get; set; } 
        public string KullaniciID { get; set; }
        public int PuanDegeri { get; set; }
        public DateTime KazanilanTarih { get; set; }
    }
}

namespace YAZLAB2.Models
{
    public class Bildirim
    {
        public int BildirimId { get; set; }

        public string KullanıcıId { get; set; }
        public int EtkinlikId { get; set; }
        public DateTime BildirimTarih { get; set; }
        public string Mesaj { get; set; }
        public bool IsAdminNotification { get; set; } 

    }



}


using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace YAZLAB2.Models
{
        public class User : IdentityUser
        {
            [Required(ErrorMessage = "T.C. Kimlik numarası girilmelidir!!!")]
            [RegularExpression(@"^\d{11}$", ErrorMessage = "T.C. Kimlik numarası harf,sembol vb. içeremez!!!")]
            //tcnoyu external servis ile authenticate et
            //fotograf yukleme sonra halledilecek
            public override string UserName { get; set; }
            public string Ad { get; set; }
            public string Soyad { get; set; }
            public string Konum { get; set; }
        public string İlgiAlan { get; set; }

        public string adres { get; set; }
           
        


            public string Email { get; set; }



        }
    }

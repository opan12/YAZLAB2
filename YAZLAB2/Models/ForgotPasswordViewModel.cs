using System.ComponentModel.DataAnnotations;

namespace YAZLAB2.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
namespace YAZLAB2.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
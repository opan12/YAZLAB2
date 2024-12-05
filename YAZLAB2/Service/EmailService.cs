using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace YAZLAB2.Service
{
    public class EmailService
    {
        public async Task SendResetPasswordEmail(string toEmail, string resetLink)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("caglagok369@gmail.com", "efur dsku pahq ckwz"), 
                EnableSsl = true 
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"), 
                Subject = "Şifre Sıfırlama Talebi",
                Body = $"Şifrenizi sıfırlamak için linke tıklayın: {resetLink}",
                IsBodyHtml = true 
            };

            mailMessage.To.Add(toEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                throw;
            }
        }

        public async Task SendConfirmationEmail(string toEmail, string confirmationCode)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, 
                Credentials = new NetworkCredential("caglagok369@gmail.com", "efur dsku pahq ckwz"),
                EnableSsl = true 
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("caglagok369@gmail.com"), 
                Subject = "Hesap Onay Kodu",
                Body = $"Hesabınızı onaylamak için aşağıdaki kodu kullanın: <strong>{confirmationCode}</strong>",
                IsBodyHtml = true 
            };

            mailMessage.To.Add(toEmail); 

            try
            {
                await smtpClient.SendMailAsync(mailMessage); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                throw; 
            }
        }
    }
}

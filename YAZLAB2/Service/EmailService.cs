using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace YAZLAB2.Service
{
    public class EmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "sudeopan3@gmail.com";  // Burada güvenlik için ortam değişkeni veya başka bir güvenli yöntem kullanılabilir
        private readonly string _smtpPass = "Sudeopan.123";  // Burada güvenlik için ortam değişkeni veya başka bir güvenli yöntem kullanılabilir

        public async Task SendResetPasswordEmail(string toEmail, string resetLink)
        {
            var smtpClient = new SmtpClient(_smtpHost)
            {
                Port = _smtpPort, // TLS için doğru port
                Credentials = new NetworkCredential(_smtpUser, _smtpPass), // Gmail hesabınızın bilgileri
                EnableSsl = true // SSL/TLS bağlantısı aktif
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser), // Gönderen e-posta adresi
                Subject = "Şifre Sıfırlama Talebi",
                Body = $"Şifrenizi sıfırlamak için linke tıklayın: <a href='{resetLink}'>Sıfırlama Linki</a>",
                IsBodyHtml = true // HTML içerik kullanıyorsanız true yapın
            };

            mailMessage.To.Add(toEmail); // Alıcı e-posta adresi

            try
            {
                await smtpClient.SendMailAsync(mailMessage); // E-posta gönder
            }
            catch (Exception ex)
            {
                // Hata loglama işlemi burada yapılabilir
                Console.WriteLine($"E-posta gönderme hatası: {ex.Message}");
                throw; // Hata tekrar fırlatılır
            }
        }
    }
}

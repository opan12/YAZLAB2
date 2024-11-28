
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using YAZLAB2.Service;
using YAZLAB2.Data;
using YAZLAB2.Models;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;  // EmailService sınıfını ekliyoruz
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(
        ApplicationDbContext context,
        EmailService emailService,  // Burada EmailService sınıfını ekliyoruz
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _emailService = emailService;  // EmailService'ı dependency injection ile alıyoruz
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Message"] = "E-posta adresi gerekli.";
            TempData["MessageType"] = "danger"; // Hata mesajı için
            return View();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return Json(new { success = false, message = "Bu e-posta adresine sahip kullanıcı bulunamadı." });
        }

        var callbackUrl = Url.Action("ResetPassword", "Account", new { email }, Request.Scheme);

        try
        {
            // E-posta gönderme işlemi
            await _emailService.SendResetPasswordEmail(user.Email, callbackUrl);
            TempData["Message"] = "Şifre sıfırlama e-postası başarıyla gönderildi.";
            TempData["MessageType"] = "success";      }
        catch (Exception ex)
        {
            // E-posta gönderme hatası

            TempData["Message"] = "E-posta gönderilirken bir hata oluştu.";
            TempData["MessageType"] = "danger"; // Hata mesajı için        
        }
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Index", "Home");
        }

        var model = new ResetPasswordViewModel { Email = email };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Geçersiz giriş." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Şifre);
        _context.Users.Update(user);

        try
        {
            await _context.SaveChangesAsync();
            await _emailService.SendResetPasswordEmail(user.Email, "Şifreniz başarıyla sıfırlandı.");
            TempData["Message"] = "Şifre başarıyla güncellendi.";
            TempData["MessageType"] = "success";
        }
        catch (Exception ex)
        {
            // Hata loglama
            return Json(new { success = false, message = "Şifre güncellenirken bir hata oluştu." });
        }
        return View(model);

    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }
}

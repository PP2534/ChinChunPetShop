using ChinChunPetShop.Models.Configs;
using ChinChunPetShop.Models.Entity;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;


namespace ChinChunPetShop.Models.Services
{
    public class SecurityService : DBConfig
    {
        private readonly SMTPSettings _SMTP;
        public SecurityService(IOptions<SMTPSettings> SMTPOptions, ChinChunPetShopContext context) : base(context)
        {
            _SMTP = SMTPOptions.Value;
        }


        public async Task SendEmailAsync2(string toEmail, string subject, string body)
        {
            using (var smtpClient = new SmtpClient("smtp.gmail.com"))
            {
                smtpClient.Port = 465; // Cổng SMTP 

                smtpClient.Credentials = new NetworkCredential(_SMTP.From, _SMTP.Password);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_SMTP.From, "ChinChun PetShop"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true, //Để gửi HTML
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromAddress = new MailAddress(_SMTP.From, "ChinChun PetShop");
            var toAddress = new MailAddress(toEmail);
            string fromPassword = _SMTP.Password;

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,               // đổi thành 587
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                // ⚠️ Bắt buộc phải bật SSL với port 465
                smtp.TargetName = "STARTTLS/smtp.gmail.com";

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }
            }
        }
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Hàm xác minh mật khẩu
        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword);
        }
        public bool checkuser(string mauser, string mavaitro)
        {
            var check = db.PhanQuyens.Where(m => m.MaNV == mauser && m.MaVT == mavaitro).SingleOrDefault();
            if (check == null) return false;
            return true;

        }

        public string EncryptPassword(string plainText, string encryptionKey)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.IV = new byte[16];
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        public string DecryptPassword(string encryptedText, string encryptionKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
                aes.IV = new byte[16];
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    byte[] decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decrypted);
                }
            }
        }
    }
}

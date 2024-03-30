using System.Net;
using System.Net.Http;
using System.Net.Mail;

namespace CodeBuggy.Models
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential("appcodebuggy@gmail.com", "iksw plht cdzd iqmv"); //Prob Needs some Encryptionhere
            smtpClient.EnableSsl = true;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("appcodebuggy@gmail.com", "CodeBuggy");
            mailMessage.To.Add(email);
            mailMessage.Subject = "Invite Access Code to CodeBuggy Project";
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = htmlMessage;
            smtpClient.Send(mailMessage);
            
            return Task.CompletedTask;
        }
    }

}

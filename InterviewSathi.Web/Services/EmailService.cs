using System.Net.Mail;
using System.Net;

namespace InterviewSathi.Web.Services
{
    public class EmailService 
    {
        public static void SendMail(string to, string sub, string body)
        {
            string fromMail = "zackzig55@gmail.com";
            string password = "cocr zhew jitc ceyk";

            using MailMessage message = new();
            message.From = new MailAddress(fromMail);
            message.Subject = sub;
            message.To.Add(new MailAddress(to));
            message.Body = $"<html><body>{body}</body></html>";
            message.IsBodyHtml = true;

            using SmtpClient smtpClient = new("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential(fromMail, password);
            smtpClient.EnableSsl = true;

            smtpClient.Send(message);
        }

    }
}

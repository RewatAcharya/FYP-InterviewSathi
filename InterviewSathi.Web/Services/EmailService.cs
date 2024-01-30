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

            MailMessage message = new()
            {
                From = new MailAddress(fromMail),
                Subject = sub 
            };
            message.To.Add(new MailAddress(to)); 
            message.Body = $"<html><body>{body}</body></html>";
            message.IsBodyHtml = true;

            var smptClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, password),
                EnableSsl = true
            };

            smptClient.Send(message);
        }
    }
}

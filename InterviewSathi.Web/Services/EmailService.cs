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
            message.Body = $@"
                <html>
                <head>
                <style>
                body {{
                    font-family: Arial, sans-serif;
                    font-size: 14px;
                    color: #333;
                }}
                .container {{
                    max-width: 600px;
                    margin: 0 auto;
                    padding: 20px;
                    border: 1px solid #ccc;
                    border-radius: 5px;
                }}
                h1 {{
                    color: #007bff;
                }}
                p {{
                    margin-bottom: 15px;
                }}
                </style>
                </head>
                <body>
                <div class='container'>
                    <img src='https://img.freepik.com/free-vector/job-interview-conversation_74855-7566.jpg' alt='Static Image' style='max-width: 100%; height: auto;'>
                    {body}
                </div>
                </body>
                </html>";
            message.IsBodyHtml = true;

            using SmtpClient smtpClient = new("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential(fromMail, password);
            smtpClient.EnableSsl = true;

            smtpClient.Send(message);
        }

    }
}

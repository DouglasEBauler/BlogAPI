using System.Net;
using System.Net.Mail;

namespace BlogAPI.Services;

public class EmailService
{
    public bool Send(
        string toName,
        string toEmail,
        string subject,
        string body,
        string fromName = "Equipe balta.io",
        string fromEmail = "email@balta.io")
    {
        var smtpClient = new SmtpClient(Configuration.Smtp.Host, Configuration.Smtp.Port)
        {
            Credentials = new NetworkCredential(Configuration.Smtp.UserName, Configuration.Smtp.Password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = true,
        };

        var email = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        email.To.Add(new MailAddress(toEmail, toName));

        try
        {
            smtpClient.Send(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

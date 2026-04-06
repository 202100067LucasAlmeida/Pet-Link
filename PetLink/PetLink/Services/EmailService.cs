using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace PetLink.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderPassword { get; set; }
        public string SenderName { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendAdoptionConfirmationAsync(string toEmail, string userName, string animalName, string shelterName)
        {
            var subject = $"Adoption Confirmation - {animalName}";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #05528F; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; color: #666; }}
                        .button {{ background-color: #05528F; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>PetLink - Adoption Confirmed! 🎉</h2>
                        </div>
                        <div class='content'>
                            <h3>Dear {userName},</h3>
                            <p>Congratulations! Your adoption of <strong>{animalName}</strong> has been successfully completed.</p>
                            <p>The pet will now be transferred to your care. The shelter <strong>{shelterName}</strong> will contact you shortly with pickup details.</p>
                            <p>Thank you for choosing adoption and giving {animalName} a loving forever home!</p>
                            <p>Best regards,<br/>PetLink Team</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 PetLink - Connecting pets with loving homes</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendNewMessageNotificationAsync(string toEmail, string receiverName, string senderName, string messagePreview)
        {
            var subject = $"New Message from {senderName} on PetLink";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #05528F; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .message {{ background-color: #f8f9fa; padding: 15px; border-radius: 10px; margin: 15px 0; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; color: #666; }}
                        .button {{ background-color: #05528F; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>PetLink - New Message! 💬</h2>
                        </div>
                        <div class='content'>
                            <h3>Hello {receiverName},</h3>
                            <p>You have received a new message from <strong>{senderName}</strong>.</p>
                            <div class='message'>
                                <p><strong>Message preview:</strong></p>
                                <p>{messagePreview}</p>
                            </div>
                            <p>Click the button below to view and reply to this message:</p>
                            <p><a href='https://localhost:5001/Messages' class='button'>Go to Messages</a></p>
                            <p>Best regards,<br/>PetLink Team</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 PetLink - Connecting pets with loving homes</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
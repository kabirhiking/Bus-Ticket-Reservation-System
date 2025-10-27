using BusTicketReservation.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace BusTicketReservation.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otpCode, string purpose)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var senderName = emailSettings["SenderName"] ?? "RapidTickets";

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email configuration is incomplete");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                var subject = purpose switch
                {
                    "LOGIN" => "Your RapidTickets Login Code",
                    "SIGNUP" => "Verify Your RapidTickets Account",
                    _ => "Your RapidTickets Verification Code"
                };

                var body = GenerateOtpEmailBody(otpCode, purpose);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"OTP email sent successfully to {email} for {purpose}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send OTP email to {email} for {purpose}");
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpHost = emailSettings["SmtpHost"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var senderName = emailSettings["SenderName"] ?? "RapidTickets";

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email configuration is incomplete");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = true
                };

                var subject = "Welcome to RapidTickets! 🎉";
                var body = GenerateWelcomeEmailBody(fullName);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Welcome email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                return false;
            }
        }

        private string GenerateOtpEmailBody(string otpCode, string purpose)
        {
            var action = purpose switch
            {
                "LOGIN" => "sign in to",
                "SIGNUP" => "verify",
                _ => "access"
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc2626; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #dc2626; text-align: center; margin: 20px 0; padding: 15px; background: white; border-radius: 8px; letter-spacing: 5px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚌 RapidTickets</h1>
        </div>
        <div class='content'>
            <h2>Your Verification Code</h2>
            <p>Hello! Use the following code to {action} your RapidTickets account:</p>
            
            <div class='otp-code'>{otpCode}</div>
            
            <p><strong>Important:</strong></p>
            <ul>
                <li>This code will expire in 10 minutes</li>
                <li>Do not share this code with anyone</li>
                <li>If you didn't request this code, please ignore this email</li>
            </ul>
            
            <p>Thank you for choosing RapidTickets for your bus travel needs!</p>
        </div>
        <div class='footer'>
            <p>© 2025 RapidTickets. All rights reserved.</p>
            <p>This is an automated message, please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeEmailBody(string fullName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc2626; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .welcome-message {{ text-align: center; margin: 20px 0; }}
        .features {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚌 Welcome to RapidTickets!</h1>
        </div>
        <div class='content'>
            <div class='welcome-message'>
                <h2>Hello {fullName}! 👋</h2>
                <p>Welcome to RapidTickets - your trusted partner for comfortable bus travel!</p>
            </div>
            
            <div class='features'>
                <h3>🎉 What you can do now:</h3>
                <ul>
                    <li>🔍 Search for bus routes across the country</li>
                    <li>🎫 Book tickets instantly with secure payment</li>
                    <li>💺 Choose your preferred seats</li>
                    <li>📱 Manage your bookings on the go</li>
                    <li>📧 Get instant booking confirmations</li>
                </ul>
            </div>
            
            <p style='text-align: center;'>
                <strong>Ready to start your journey?</strong><br>
                Visit our website and book your first trip today!
            </p>
        </div>
        <div class='footer'>
            <p>© 2025 RapidTickets. All rights reserved.</p>
            <p>Thank you for choosing RapidTickets for your travel needs!</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
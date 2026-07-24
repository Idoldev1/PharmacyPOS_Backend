using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        _logger.LogInformation("Sending OTP email to {Email}", toEmail);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Your Password Reset Code";
        message.Body = new TextPart("plain")
        {
            Text = $"Your password reset code is: {otp}\n\n" +
                   $"This code expires in 10 minutes.\n\n" +
                   $"If you did not request a password reset, please ignore this email."
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);

        _logger.LogInformation("OTP email sent to {Email}", toEmail);
    }
}

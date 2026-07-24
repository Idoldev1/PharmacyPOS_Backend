namespace POS.API.Services.Contracts;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string otp);
}

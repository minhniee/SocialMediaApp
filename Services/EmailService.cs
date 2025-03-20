using Microsoft.Extensions.Options;
using SocialMediaApp.Config;
using System.Net;
using System.Net.Mail;

namespace SocialMediaApp.Services;

public class EmailService : IEmailService
{

	private readonly EmailConfiguration _emailConfig;
	private readonly IConfiguration _configuration;

	public EmailService(IOptions<EmailConfiguration> emailConfig, IConfiguration configuration)
	{
		_emailConfig = emailConfig.Value;
		_configuration = configuration;
	}

	public async Task SendEmailAsync(string email, string subject, string message)
	{
		try
		{
			var mail = new MailMessage
			{
				From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName),
				Subject = subject,
				Body = message,
				IsBodyHtml = true
			};

			mail.To.Add(new MailAddress(email));

			using (var smtp = new SmtpClient(_emailConfig.SmtpServer, _emailConfig.SmtpPort))
			{
				smtp.Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
				//smtp.UseDefaultCredentials = true;
				smtp.EnableSsl = _emailConfig.EnableSsl;

				await smtp.SendMailAsync(mail);
			}
			Console.Write(_emailConfig.ToString());
		}
		catch (Exception ex)
		{
			// Log the error
			throw new Exception($"Failed to send email: {ex.Message}", ex);
		}
	}

	public async Task SendPasswordResetEmailAsync(string email, string token)
	{
		var resetLink = $"http://localhost:5150/Auth/ResetPassword?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(email)}";

		var subject = "Đặt lại mật khẩu";
		var message = GetPasswordResetEmailTemplate(resetLink);

		await SendEmailAsync(email, subject, message);
	}

	public async Task SendWelcomeEmailAsync(string email, string userName)
	{
		var subject = "Chào mừng bạn đến với Social Media";
		var message = GetWelcomeEmailTemplate(userName);

		await SendEmailAsync(email, subject, message);
	}

	public Task SendSendVerificationEmaillAsync(string email, string userName)
	{
		throw new NotImplementedException();
	}


	private string GetPasswordResetEmailTemplate(string resetLink)
	{
		return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2>Đặt lại mật khẩu</h2>
                        <p>Bạn đã yêu cầu đặt lại mật khẩu. Vui lòng click vào link bên dưới để đặt lại mật khẩu:</p>
                        <p>
                            <a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                Đặt lại mật khẩu
                            </a>
                        </p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <p>Link này sẽ hết hạn sau 24 giờ.</p>
                    </div>
                </body>
                </html>";
	}

	private string GetWelcomeEmailTemplate(string userName)
	{
		return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2>Chào mừng {userName}!</h2>
                        <p>Cảm ơn bạn đã đăng ký tài khoản tại Social Media.</p>
                        <p>Chúng tôi rất vui được chào đón bạn vào cộng đồng của chúng tôi.</p>
                        <p>Hãy bắt đầu bằng việc:</p>
                        <ul>
                            <li>Cập nhật thông tin cá nhân</li>
                            <li>Kết nối với bạn bè</li>
                            <li>Chia sẻ những khoảnh khắc đáng nhớ</li>
                        </ul>
                    </div>
                </body>
                </html>";
	}
}
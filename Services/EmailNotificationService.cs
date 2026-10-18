using System.Net;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MarkGraphiko.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IConfiguration config, ILogger<EmailNotificationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, string provider)
    {
        string subject = "Welcome to MARK GRAPHIKO | Creative Studio Access Granted";
        string html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0b0f19; color: #f1f5f9; margin: 0; padding: 20px; }}
  .card {{ max-width: 580px; margin: 0 auto; background: #111827; border: 1px solid #1f2937; border-radius: 16px; overflow: hidden; box-shadow: 0 20px 40px rgba(0,0,0,0.5); }}
  .header {{ background: linear-gradient(135deg, #4f46e5, #06b6d4); padding: 40px 30px; text-align: center; }}
  .brand {{ font-size: 26px; font-weight: 800; letter-spacing: 2px; color: #ffffff; text-transform: uppercase; margin: 0; }}
  .tagline {{ font-size: 13px; color: rgba(255,255,255,0.85); letter-spacing: 1px; margin-top: 6px; }}
  .content {{ padding: 36px 30px; }}
  .greeting {{ font-size: 20px; font-weight: 600; color: #ffffff; margin-bottom: 16px; }}
  .text {{ font-size: 15px; line-height: 1.7; color: #94a3b8; margin-bottom: 24px; }}
  .badge {{ display: inline-block; padding: 6px 14px; background: #1e1b4b; border: 1px solid #6366f1; border-radius: 9999px; color: #a5b4fc; font-size: 12px; font-weight: 600; }}
  .btn {{ display: inline-block; background: linear-gradient(135deg, #6366f1, #3b82f6); color: #ffffff; text-decoration: none; padding: 14px 28px; border-radius: 10px; font-weight: 600; font-size: 14px; letter-spacing: 0.5px; box-shadow: 0 4px 15px rgba(99,102,241,0.4); }}
  .footer {{ padding: 24px 30px; text-align: center; font-size: 12px; color: #64748b; border-top: 1px solid #1f2937; }}
</style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h1 class='brand'>MARK GRAPHIKO</h1>
      <p class='tagline'>PREMIUM GRAPHIC DESIGN &amp; BRAND ARCHITECTURE</p>
    </div>
    <div class='content'>
      <div class='greeting'>Hello, {WebUtility.HtmlEncode(userName)}! 👋</div>
      <p class='text'>
        Your account on <strong>MARK GRAPHIKO</strong> has been registered successfully via 
        <span class='badge'>{WebUtility.HtmlEncode(provider)}</span>.
      </p>
      <p class='text'>
        You now have exclusive client access to browse our full portfolio archive, save and bookmark favorite designs, and send direct project inquiries for custom apparel, t-shirt mockups, brand logos, and vector illustrations.
      </p>
      <div style='text-align: center; margin: 32px 0;'>
        <a href='#' class='btn'>Explore Design Gallery</a>
      </div>
    </div>
    <div class='footer'>
      &copy; {DateTime.UtcNow.Year} MARK GRAPHIKO Studio. All rights reserved.<br>
      Direct client inquiries: studio@markgraphiko.com
    </div>
  </div>
</body>
</html>";

        await SendCustomEmailAsync(toEmail, subject, html);
    }

    public async Task SendLoginNotificationAsync(string toEmail, string userName, string ipAddress, string userAgent)
    {
        string subject = "Security Alert: New Sign-in to Your MARK GRAPHIKO Account";
        string html = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background: #0b0f19; color: #f8fafc; padding: 20px;'>
  <div style='max-width: 520px; margin: auto; background: #111827; border: 1px solid #1f2937; border-radius: 12px; padding: 28px;'>
    <h2 style='color: #38bdf8; margin-top: 0;'>New Sign-In Detected</h2>
    <p style='color: #94a3b8; font-size: 14px;'>Hello {WebUtility.HtmlEncode(userName)}, your account recently logged in from:</p>
    <div style='background: #1e293b; border-radius: 8px; padding: 14px; font-size: 13px; color: #cbd5e1; margin: 16px 0;'>
      <strong>Time (UTC):</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}<br>
      <strong>IP Address:</strong> {WebUtility.HtmlEncode(ipAddress)}<br>
      <strong>Device / Client:</strong> {WebUtility.HtmlEncode(userAgent)}
    </div>
    <p style='color: #64748b; font-size: 12px;'>If this was you, no action is needed. If you did not initiate this login, please secure your credentials immediately.</p>
  </div>
</body>
</html>";

        await SendCustomEmailAsync(toEmail, subject, html);
    }

    public async Task SendInquiryConfirmationAsync(string toEmail, string userName, string subject, string projectType)
    {
        string emailSubject = $"Inquiry Received: {subject} | MARK GRAPHIKO";
        string html = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background: #0f172a; color: #f8fafc; padding: 24px;'>
  <div style='max-width: 560px; margin: auto; background: #1e293b; border: 1px solid #334155; border-radius: 14px; padding: 32px;'>
    <h2 style='color: #818cf8; margin-top: 0;'>Inquiry Received! 🚀</h2>
    <p style='color: #cbd5e1; font-size: 15px; line-height: 1.6;'>
      Hi {WebUtility.HtmlEncode(userName)},<br><br>
      Thank you for reaching out to <strong>MARK GRAPHIKO</strong>. I have received your project inquiry for 
      <strong style='color: #38bdf8;'>{WebUtility.HtmlEncode(projectType)}</strong> regarding <em>'{WebUtility.HtmlEncode(subject)}'</em>.
    </p>
    <p style='color: #94a3b8; font-size: 14px; line-height: 1.6;'>
      I personally review every project request and will review your specifications, mood board, or references. You can expect a response within 24 hours.
    </p>
    <div style='margin-top: 24px; padding-top: 16px; border-top: 1px solid #334155; font-size: 12px; color: #64748b;'>
      MARK GRAPHIKO &bull; Graphic Designer &amp; Illustrator &bull; studio@markgraphiko.com
    </div>
  </div>
</body>
</html>";

        await SendCustomEmailAsync(toEmail, emailSubject, html);
    }

    public async Task SendAdminInquiryAlertAsync(string clientName, string clientEmail, string subject, string content, string projectType, string budget)
    {
        string adminEmail = _config["AdminAccount:Email"] ?? "admin@markgraphiko.com";
        string emailSubject = $"🔥 New Project Inquiry from {clientName} - [{projectType}]";
        string html = $@"
<!DOCTYPE html>
<html>
<body style='font-family: Arial, sans-serif; background: #030712; color: #f9fafb; padding: 24px;'>
  <div style='max-width: 600px; margin: auto; background: #111827; border: 1px solid #374151; border-radius: 14px; padding: 32px;'>
    <span style='background: #f59e0b; color: #000; font-size: 11px; font-weight: bold; padding: 4px 8px; border-radius: 4px; text-transform: uppercase;'>New Client Inquiry</span>
    <h2 style='color: #ffffff; margin: 12px 0 20px;'>{WebUtility.HtmlEncode(subject)}</h2>
    
    <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px; font-size: 14px;'>
      <tr><td style='color: #9ca3af; padding: 6px 0; width: 120px;'>Client:</td><td style='color: #ffffff; font-weight: 600;'>{WebUtility.HtmlEncode(clientName)}</td></tr>
      <tr><td style='color: #9ca3af; padding: 6px 0;'>Email:</td><td style='color: #38bdf8;'>{WebUtility.HtmlEncode(clientEmail)}</td></tr>
      <tr><td style='color: #9ca3af; padding: 6px 0;'>Project Type:</td><td style='color: #a78bfa;'>{WebUtility.HtmlEncode(projectType)}</td></tr>
      <tr><td style='color: #9ca3af; padding: 6px 0;'>Budget:</td><td style='color: #34d399; font-weight: 600;'>{WebUtility.HtmlEncode(budget)}</td></tr>
    </table>

    <div style='background: #1f2937; border-left: 4px solid #6366f1; border-radius: 6px; padding: 16px; margin: 20px 0;'>
      <div style='font-size: 12px; color: #9ca3af; margin-bottom: 8px;'>Message Content:</div>
      <div style='color: #f3f4f6; font-size: 14px; line-height: 1.6; white-space: pre-wrap;'>{WebUtility.HtmlEncode(content)}</div>
    </div>
  </div>
</body>
</html>";

        await SendCustomEmailAsync(adminEmail, emailSubject, html);
    }

    public async Task SendPasswordResetCodeAsync(string toEmail, string userName, string resetCode)
    {
        string subject = "Your Password Modification Verification Code | MARK GRAPHIKO";
        string html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0b0f19; color: #f1f5f9; margin: 0; padding: 20px; }}
  .card {{ max-width: 580px; margin: 0 auto; background: #111827; border: 1px solid #1f2937; border-radius: 16px; overflow: hidden; box-shadow: 0 20px 40px rgba(0,0,0,0.5); }}
  .header {{ background: linear-gradient(135deg, #6366f1, #a855f7); padding: 36px 30px; text-align: center; }}
  .brand {{ font-size: 24px; font-weight: 800; letter-spacing: 2px; color: #ffffff; text-transform: uppercase; margin: 0; }}
  .tagline {{ font-size: 12px; color: rgba(255,255,255,0.85); letter-spacing: 1px; margin-top: 4px; }}
  .content {{ padding: 32px 30px; }}
  .greeting {{ font-size: 18px; font-weight: 600; color: #ffffff; margin-bottom: 12px; }}
  .text {{ font-size: 14px; line-height: 1.6; color: #94a3b8; margin-bottom: 20px; }}
  .code-box {{ text-align: center; background: #0b0f19; border: 2px dashed #6366f1; border-radius: 12px; padding: 20px; margin: 24px 0; }}
  .code {{ font-size: 36px; font-weight: 900; letter-spacing: 10px; color: #38bdf8; font-family: 'Courier New', monospace; }}
  .alert {{ background: rgba(245, 158, 11, 0.1); border-left: 4px solid #f59e0b; padding: 12px 16px; border-radius: 6px; font-size: 12px; color: #fbbf24; margin-top: 16px; }}
  .footer {{ padding: 20px 30px; text-align: center; font-size: 11px; color: #64748b; border-top: 1px solid #1f2937; }}
</style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h1 class='brand'>MARK GRAPHIKO</h1>
      <p class='tagline'>SECURITY &amp; IDENTITY AUTHENTICATION</p>
    </div>
    <div class='content'>
      <div class='greeting'>Hello, {WebUtility.HtmlEncode(userName)}</div>
      <p class='text'>
        We received a request to modify or reset the password for your <strong>MARK GRAPHIKO</strong> account associated with <strong>{WebUtility.HtmlEncode(toEmail)}</strong>.
      </p>
      <p class='text'>Use the 6-digit verification code below to authorize this password change:</p>
      
      <div class='code-box'>
        <div style='font-size: 11px; color: #94a3b8; text-transform: uppercase; letter-spacing: 2px; margin-bottom: 6px;'>Verification Code</div>
        <div class='code'>{resetCode}</div>
        <div style='font-size: 11px; color: #94a3b8; margin-top: 6px;'>Valid for 15 minutes</div>
      </div>

      <div class='alert'>
        <strong>Important:</strong> If you did not initiate this request, someone may be trying to access your account. Please keep this code confidential and do not share it with anyone.
      </div>
    </div>
    <div class='footer'>
      &copy; {DateTime.UtcNow.Year} MARK GRAPHIKO Studio. All rights reserved.
    </div>
  </div>
</body>
</html>";

        await SendCustomEmailAsync(toEmail, subject, html);
    }

    public async Task SendPasswordChangedNotificationAsync(string toEmail, string userName)
    {
        string subject = "Security Alert: Your Password Was Changed | MARK GRAPHIKO";
        string html = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #0b0f19; color: #f1f5f9; margin: 0; padding: 20px; }}
  .card {{ max-width: 580px; margin: 0 auto; background: #111827; border: 1px solid #1f2937; border-radius: 16px; overflow: hidden; }}
  .header {{ background: linear-gradient(135deg, #10b981, #06b6d4); padding: 30px; text-align: center; }}
  .brand {{ font-size: 22px; font-weight: 800; color: #ffffff; text-transform: uppercase; margin: 0; }}
  .content {{ padding: 30px; }}
  .text {{ font-size: 14px; line-height: 1.6; color: #94a3b8; }}
  .footer {{ padding: 20px 30px; text-align: center; font-size: 11px; color: #64748b; border-top: 1px solid #1f2937; }}
</style>
</head>
<body>
  <div class='card'>
    <div class='header'>
      <h1 class='brand'>MARK GRAPHIKO</h1>
    </div>
    <div class='content'>
      <h2 style='color:#ffffff; font-size:18px; margin-top:0;'>Password Successfully Updated</h2>
      <p class='text'>Hello {WebUtility.HtmlEncode(userName)},</p>
      <p class='text'>
        The password for your MARK GRAPHIKO account (<strong>{WebUtility.HtmlEncode(toEmail)}</strong>) was modified on {DateTime.UtcNow.ToString("MMMM dd, yyyy HH:mm")} UTC.
      </p>
      <p class='text'>
        If you performed this action, no further steps are needed. If you did not make this change, please contact Mark Graphiko Studio immediately at admin@markgraphiko.com.
      </p>
    </div>
    <div class='footer'>
      &copy; {DateTime.UtcNow.Year} MARK GRAPHIKO Studio.
    </div>
  </div>
</body>
</html>";

        await SendCustomEmailAsync(toEmail, subject, html);
    }

    public async Task SendCustomEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        string fromEmail = _config["EmailSettings:FromEmail"] ?? "noreply@markgraphiko.com";
        string fromName = _config["EmailSettings:FromName"] ?? "MARK GRAPHIKO Studio";
        string? sendGridKey = _config["SendGrid:ApiKey"];

        if (!string.IsNullOrWhiteSpace(sendGridKey) && sendGridKey != "YOUR_SENDGRID_API_KEY")
        {
            try
            {
                var client = new SendGridClient(sendGridKey);
                var from = new EmailAddress(fromEmail, fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlMessage);
                var response = await client.SendEmailAsync(msg);
                _logger.LogInformation("SendGrid email sent to {Email}. Status: {StatusCode}", toEmail, response.StatusCode);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SendGrid to {Email}", toEmail);
            }
        }

        // Check SMTP configuration fallback
        string? smtpHost = _config["Smtp:Host"];
        if (!string.IsNullOrWhiteSpace(smtpHost) && smtpHost != "smtp.example.com")
        {
            try
            {
                int smtpPort = _config.GetValue<int>("Smtp:Port", 587);
                string? smtpUser = _config["Smtp:Username"];
                string? smtpPass = _config["Smtp:Password"];
                bool enableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true);

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    Credentials = !string.IsNullOrEmpty(smtpUser) ? new NetworkCredential(smtpUser, smtpPass) : null
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("SMTP email sent to {Email}", toEmail);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP to {Email}", toEmail);
            }
        }

        // In development / demo environment without active live credentials:
        // Gracefully simulate dispatch and log output so users test the full flow seamlessly!
        _logger.LogInformation("=== [EMAIL SIMULATED OUTBOX] ===");
        _logger.LogInformation("To: {ToEmail}", toEmail);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Provider: Simulated Dispatch (Configure SendGrid:ApiKey or Smtp in appsettings.json for live SMTP delivery)");
        _logger.LogInformation("================================");
    }
}

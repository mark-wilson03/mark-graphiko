namespace MarkGraphiko.Services;

public interface IEmailNotificationService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName, string provider);
    Task SendLoginNotificationAsync(string toEmail, string userName, string ipAddress, string userAgent);
    Task SendInquiryConfirmationAsync(string toEmail, string userName, string subject, string projectType);
    Task SendAdminInquiryAlertAsync(string clientName, string clientEmail, string subject, string content, string projectType, string budget);
    Task SendPasswordResetCodeAsync(string toEmail, string userName, string resetCode);
    Task SendPasswordChangedNotificationAsync(string toEmail, string userName);
    Task SendCustomEmailAsync(string toEmail, string subject, string htmlMessage);
}

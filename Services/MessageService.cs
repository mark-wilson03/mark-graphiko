using Microsoft.EntityFrameworkCore;
using MarkGraphiko.Data;
using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;

namespace MarkGraphiko.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailNotificationService _emailService;

    public MessageService(ApplicationDbContext context, IEmailNotificationService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Message> SendMessageAsync(MessageInputModel model, string? userId)
    {
        var message = new Message
        {
            SenderId = userId,
            SenderName = model.SenderName.Trim(),
            SenderEmail = model.SenderEmail.Trim(),
            PhoneNumber = model.PhoneNumber?.Trim(),
            Subject = model.Subject.Trim(),
            Content = model.Content.Trim(),
            ProjectType = model.ProjectType,
            BudgetRange = model.BudgetRange,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Trigger asynchronous email notifications
        _ = Task.Run(async () =>
        {
            try
            {
                // Confirmation to the client
                await _emailService.SendInquiryConfirmationAsync(
                    message.SenderEmail,
                    message.SenderName,
                    message.Subject,
                    message.ProjectType);

                // Alert notification to the admin
                await _emailService.SendAdminInquiryAlertAsync(
                    message.SenderName,
                    message.SenderEmail,
                    message.Subject,
                    message.Content,
                    message.ProjectType,
                    message.BudgetRange);
            }
            catch
            {
                // Background email delivery caught cleanly
            }
        });

        return message;
    }

    public async Task<List<Message>> GetAllMessagesAsync()
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => !m.IsDeletedByAdmin && !m.IsArchived)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetUnreadMessagesAsync()
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => !m.IsDeletedByAdmin && !m.IsArchived && !m.IsRead)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetAdminArchivedMessagesAsync()
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .Where(m => !m.IsDeletedByAdmin && m.IsArchived)
            .OrderByDescending(m => m.ArchivedAt ?? m.SentAt)
            .ToListAsync();
    }

    public async Task<Message?> GetMessageByIdAsync(int id)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var msg = await _context.Messages.FindAsync(id);
        if (msg == null) return false;

        msg.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReplyToMessageAsync(int id, string replyText)
    {
        var msg = await _context.Messages.FindAsync(id);
        if (msg == null) return false;

        msg.AdminReply = replyText.Trim();
        msg.RepliedAt = DateTime.UtcNow;
        msg.IsRead = true;
        await _context.SaveChangesAsync();

        // Send reply email to client
        _ = Task.Run(async () =>
        {
            try
            {
                string replySubject = $"Re: {msg.Subject} | Response from Mark Graphiko";
                string html = $@"
<div style='font-family: Arial, sans-serif; background: #0f172a; color: #f8fafc; padding: 24px;'>
  <div style='max-width: 560px; margin: auto; background: #1e293b; border-radius: 12px; padding: 28px; border: 1px solid #334155;'>
    <h3 style='color: #38bdf8; margin-top: 0;'>Mark Graphiko has replied to your inquiry:</h3>
    <p style='color: #e2e8f0; font-size: 15px; line-height: 1.6; white-space: pre-wrap;'>{System.Net.WebUtility.HtmlEncode(replyText)}</p>
    <hr style='border: none; border-top: 1px solid #334155; margin: 20px 0;'>
    <p style='color: #94a3b8; font-size: 12px;'>Original message: {System.Net.WebUtility.HtmlEncode(msg.Content)}</p>
  </div>
</div>";
                await _emailService.SendCustomEmailAsync(msg.SenderEmail, replySubject, html);
            }
            catch
            {
            }
        });

        return true;
    }

    public async Task<List<Message>> GetUserMessagesAsync(string userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId == userId && !m.IsDeletedByUser)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetUserActiveMessagesAsync(string userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId == userId && !m.IsDeletedByUser && !m.IsArchived)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetUserArchivedMessagesAsync(string userId)
    {
        return await _context.Messages
            .Where(m => m.SenderId == userId && !m.IsDeletedByUser && m.IsArchived)
            .OrderByDescending(m => m.ArchivedAt ?? m.SentAt)
            .ToListAsync();
    }

    public async Task<bool> ArchiveMessageAsync(int id, string userId, bool isAdmin)
    {
        var msg = await _context.Messages.FindAsync(id);
        if (msg == null) return false;
        if (!isAdmin && msg.SenderId != userId) return false;

        msg.IsArchived = true;
        msg.ArchivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnarchiveMessageAsync(int id, string userId, bool isAdmin)
    {
        var msg = await _context.Messages.FindAsync(id);
        if (msg == null) return false;
        if (!isAdmin && msg.SenderId != userId) return false;

        msg.IsArchived = false;
        msg.ArchivedAt = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMessageAsync(int id, string userId, bool isAdmin, bool permanent = true)
    {
        var msg = await _context.Messages.FindAsync(id);
        if (msg == null) return false;
        if (!isAdmin && msg.SenderId != userId) return false;

        if (!permanent)
        {
            // Soft-archive instead
            msg.IsArchived = true;
            msg.ArchivedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        if (isAdmin)
        {
            msg.IsDeletedByAdmin = true;
        }
        else
        {
            msg.IsDeletedByUser = true;
        }

        // If either permanently requested or deleted by both parties, remove entity completely
        if ((msg.IsDeletedByUser && msg.IsDeletedByAdmin) || isAdmin)
        {
            _context.Messages.Remove(msg);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.Messages.CountAsync(m => !m.IsRead && !m.IsArchived && !m.IsDeletedByAdmin);
    }
}

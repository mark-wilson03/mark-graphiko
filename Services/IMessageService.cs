using MarkGraphiko.Models;
using MarkGraphiko.Models.ViewModels;

namespace MarkGraphiko.Services;

public interface IMessageService
{
    Task<Message> SendMessageAsync(MessageInputModel model, string? userId);
    Task<List<Message>> GetAllMessagesAsync();
    Task<List<Message>> GetUnreadMessagesAsync();
    Task<List<Message>> GetAdminArchivedMessagesAsync();
    Task<Message?> GetMessageByIdAsync(int id);
    Task<bool> MarkAsReadAsync(int id);
    Task<bool> ReplyToMessageAsync(int id, string replyText);
    Task<List<Message>> GetUserMessagesAsync(string userId);
    Task<List<Message>> GetUserActiveMessagesAsync(string userId);
    Task<List<Message>> GetUserArchivedMessagesAsync(string userId);
    Task<bool> ArchiveMessageAsync(int id, string userId, bool isAdmin);
    Task<bool> UnarchiveMessageAsync(int id, string userId, bool isAdmin);
    Task<bool> DeleteMessageAsync(int id, string userId, bool isAdmin, bool permanent = true);
    Task<int> GetUnreadCountAsync();
}

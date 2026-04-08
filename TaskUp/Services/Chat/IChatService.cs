using TaskUp.Models;

namespace TaskUp.Services.Chat;

public interface IChatService
{
    Task<ChatMessage> SendMessageAsync(int boardId, string userId, string userName, string? content, List<IFormFile>? attachments);
    Task<List<ChatMessage>> GetMessagesAsync(int boardId, int count = 50);
    Task<ChatAttachment?> GetAttachmentAsync(int attachmentId);
    Task<bool> IsUserInBoardAsync(int boardId, string userId);
}
using Microsoft.EntityFrameworkCore;
using TaskUp.Data;
using TaskUp.Models;
using TaskUp.Utilities.Enums;
using TaskUp.Utilities.Extensions;
using TaskUp.Utilities.Helpers;

namespace TaskUp.Services.Chat;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ChatService> _logger;

    public ChatService(AppDbContext context, IWebHostEnvironment env, ILogger<ChatService> logger)
    {
        _context = context;
        _env = env;
        _logger = logger;
    }

   public async Task<ChatMessage> SendMessageAsync(int boardId, string userId, string userName, string? content, List<IFormFile>? attachments)
{
    if (string.IsNullOrWhiteSpace(content) && (attachments == null || !attachments.Any()))
        throw new ArgumentException("Message content or at least one attachment is required.");

    var message = new ChatMessage
    {
        BoardId = boardId,
        UserId = userId,
        UserName = userName,
        Content = content,
        CreatedAt = DateTime.UtcNow
    };

    _context.ChatMessages.Add(message);
    await _context.SaveChangesAsync(); 

    if (attachments != null && attachments.Any())
    {
        var uploadPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "chat", boardId.ToString());
        
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        foreach (var file in attachments)
        {
            if (file.Length > 0)
            {
                try
                {
                    var uniqueFileName = FileHelper.GenerateUniqueFileName(file.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var attachment = new ChatAttachment
                    {
                        ChatMessageId = message.Id,
                        FileName = file.FileName,
                        FilePath = $"/uploads/chat/{boardId}/{uniqueFileName}",
                        FileSize = file.Length,
                        ContentType = file.ContentType,
                        FileType = file.FileName.GetFileType(),
                        UploadedAt = DateTime.UtcNow,
                        UploadedBy = userId
                    };

                    _context.ChatAttachments.Add(attachment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error saving file: {file.FileName}");
                    throw; 
                }
            }
        }
        
        await _context.SaveChangesAsync(); 
    }

    return message;
}

public async Task<List<ChatMessage>> GetMessagesAsync(int boardId, int count = 50)
{
    return await _context.ChatMessages
        .Include(m => m.Attachments)  
        .Where(m => m.BoardId == boardId)
        .OrderByDescending(m => m.CreatedAt)
        .Take(count)
        .OrderBy(m => m.CreatedAt)
        .ToListAsync();
}

    public async Task<ChatAttachment?> GetAttachmentAsync(int attachmentId)
    {
        return await _context.ChatAttachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId);
    }

    public async Task<bool> IsUserInBoardAsync(int boardId, string userId)
    {
        return await _context.BoardMembers
            .AnyAsync(m => m.BoardId == boardId && m.UserId == userId);
    }
}
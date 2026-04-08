using System.ComponentModel.DataAnnotations;
using TaskUp.Utilities.Enums;

namespace TaskUp.Models;

public class ChatMessage
{
    public int Id { get; set; }
    
    [Required]
    public int BoardId { get; set; }
    public Board Board { get; set; }
    
    [Required]
    public string UserId { get; set; }
    public AppUser User { get; set; }
    
    [Required]
    public string UserName { get; set; }
    
    public string? Content { get; set; } 
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<ChatAttachment> Attachments { get; set; } = new List<ChatAttachment>();
}

public class ChatAttachment
{
    public int Id { get; set; }
    
    [Required]
    public int ChatMessageId { get; set; }
    public ChatMessage ChatMessage { get; set; }
    
    [Required]
    [StringLength(500)]  
    public string FileName { get; set; }
    
    [Required]
    [StringLength(1000)]  
    public string FilePath { get; set; }
    
    public long FileSize { get; set; }
    
    [StringLength(500)]  
    public string ContentType { get; set; }
    
    public FileType FileType { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public string? UploadedBy { get; set; }
}
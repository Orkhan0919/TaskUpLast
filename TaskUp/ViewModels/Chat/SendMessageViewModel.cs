using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TaskUp.Utilities.Attributes;

namespace TaskUp.ViewModels.Chat;

public class SendMessageViewModel
{
    [Required]
    public int BoardId { get; set; }
    
    public string? Content { get; set; }
    
    [MaxFileSize(20, "MB")]
    public List<IFormFile>? Attachments { get; set; }
}
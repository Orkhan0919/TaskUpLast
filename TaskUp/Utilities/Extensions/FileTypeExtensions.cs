using TaskUp.Utilities.Enums;

namespace TaskUp.Utilities.Extensions;

public static class FileTypeExtensions
{
    public static FileType GetFileType(this string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" => FileType.Image,
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" or ".md" => FileType.Document,
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".mkv" => FileType.Video,
            ".mp3" or ".wav" or ".ogg" or ".flac" or ".aac" => FileType.Audio,
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => FileType.Archive,
            _ => FileType.Other
        };
    }
    
    public static string GetIconClass(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => "fa-file-image",
            FileType.Document => "fa-file-lines",
            FileType.Video => "fa-file-video",
            FileType.Audio => "fa-file-audio",
            FileType.Archive => "fa-file-zipper",
            _ => "fa-file"
        };
    }
    
    public static string GetColorClass(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Image => "text-blue-400",
            FileType.Document => "text-green-400",
            FileType.Video => "text-purple-400",
            FileType.Audio => "text-yellow-400",
            FileType.Archive => "text-orange-400",
            _ => "text-gray-400"
        };
    }
    
    public static (FileType Type, string Icon, string Color) GetFileInfo(this string fileName)
    {
        var type = fileName.GetFileType();
        return (type, type.GetIconClass(), type.GetColorClass());
    }
}
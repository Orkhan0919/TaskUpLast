using TaskUp.Utilities.Enums;
using TaskUp.Utilities.Extensions;

namespace TaskUp.Utilities.Helpers;

public static class FileHelper
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
    private static readonly string[] DocumentExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".md" };
    private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".aac" };
    private static readonly string[] ArchiveExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz" };

    public static bool IsValidFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return ImageExtensions.Contains(extension) ||
               DocumentExtensions.Contains(extension) ||
               VideoExtensions.Contains(extension) ||
               AudioExtensions.Contains(extension) ||
               ArchiveExtensions.Contains(extension);
    }

    public static string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        return uniqueName;
    }

    public static async Task<string> SaveFileAsync(IFormFile file, string uploadPath)
    {
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var uniqueFileName = GenerateUniqueFileName(file.FileName);
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return uniqueFileName;
    }

    public static string GetFileSizeString(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public static string GetFileIcon(string fileName)
    {
        var fileType = FileTypeExtensions.GetFileType(fileName);
        return FileTypeExtensions.GetIconClass(fileType);
    }

    public static string GetFileColor(string fileName)
    {
        var fileType = FileTypeExtensions.GetFileType(fileName);
        return FileTypeExtensions.GetColorClass(fileType);
    }
}
using System.ComponentModel.DataAnnotations;

namespace TaskUp.Utilities.Attributes;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;
    private readonly string _unit;

    public MaxFileSizeAttribute(int maxFileSize, string unit = "MB")
    {
        _maxFileSize = maxFileSize;
        _unit = unit;
        
        int multiplier = unit.ToUpper() switch
        {
            "KB" => 1024,
            "MB" => 1024 * 1024,
            "GB" => 1024 * 1024 * 1024,
            _ => 1
        };
        
        MaxBytes = maxFileSize * multiplier;
    }

    public int MaxBytes { get; }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is IFormFile file)
        {
            if (file.Length > MaxBytes)
            {
                return new ValidationResult($"File size cannot exceed {_maxFileSize} {_unit}.");
            }
        }
        else if (value is List<IFormFile> files)
        {
            foreach (var f in files)
            {
                if (f.Length > MaxBytes)
                {
                    return new ValidationResult($"Each file size cannot exceed {_maxFileSize} {_unit}.");
                }
            }
        }

        return ValidationResult.Success;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Application.UseCase;


public interface IFileProcessor
{
    Task<string?> SaveBase64FileAsync(string base64String, string folderName);
}
public class ProcessFile : IFileProcessor
{
    public async Task<string?> SaveBase64FileAsync(string base64String, string folderName)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        var base64Parts = base64String.Split(',');

        if (base64Parts.Length != 2)
            throw new FormatException("Formato Base64 inválido.");

        var base64Data = base64Parts[1];
        var bytes = Convert.FromBase64String(base64Data);

        var extension = ".png";
        if (base64Parts[0].Contains("jpeg")) extension = ".jpg";
        else if (base64Parts[0].Contains("pdf")) extension = ".pdf";
        else if (base64Parts[0].Contains("mp4")) extension = ".mp4";

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

        await File.WriteAllBytesAsync(fullPath, bytes);

        return $"/uploads/{folderName}/{uniqueFileName}";
    }
}

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Services;

public interface IFileStorageService
{
    Task <string> SaveFileAsync(IFormFile file, string directory);
    Task DeleteFileAsync(string filePath);

    Task <string> GetFileUrlAsync(string filePath);

    Task <bool> ValidateFileAsync(IFormFile file, string[] allowedExtensions, long maxSizeInBytes);
}

public class FileStorageService : IFileStorageService
{
    private readonly string _baseUploadPath;

    public FileStorageService()
    {
        _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(_baseUploadPath))
        {
            Directory.CreateDirectory(_baseUploadPath);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("El archivo está vacío o no es válido.");
        }

        var folderPath = Path.Combine(_baseUploadPath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{folder}/{uniqueFileName}";
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        await Task.Run(() =>
        {
            try
            {
                var fullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
            }
        });
    }

    public Task<string> GetFileUrlAsync(string filePath)
    {
        // En local, la URL es la misma ruta relativa
        // En cloud sería la URL completa del blob storage
        return Task.FromResult(filePath);
    }

    public Task<bool> ValidateFileAsync(IFormFile file, string[] allowedExtensions, long maxSizeInBytes)
    {
        if (file == null || file.Length == 0)
            return Task.FromResult(false);


        if (file.Length > maxSizeInBytes)
            return Task.FromResult(false);


        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isValid = allowedExtensions.Contains(fileExtension);

        return Task.FromResult(isValid);
    }
}

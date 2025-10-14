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

    Task<FileStorageResult> SavePhotoAsync(
    IFormFile file,
    string folder,
    string[] allowedExtensions,
    long maxFileSize);

}

public class FileStorageResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
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

    public async Task<FileStorageResult> SavePhotoAsync(IFormFile file, string folder, string[] allowedExtensions, long maxFileSize)
    {
        try
        {
            var isValid = await ValidateFileAsync(file, allowedExtensions, maxFileSize);

            if (!isValid)
            {
                if (file == null || file.Length == 0)
                {
                    return new FileStorageResult
                    {
                        Success = false,
                        ErrorMessage = "No se proporcionó ningún archivo"
                    };
                }

                if (file.Length > maxFileSize)
                {
                    var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                    return new FileStorageResult
                    {
                        Success = false,
                        ErrorMessage = $"El archivo excede el tamaño máximo de {maxSizeMB:F2} MB"
                    };
                }

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                return new FileStorageResult
                {
                    Success = false,
                    ErrorMessage = $"Extensión no permitida. Solo se aceptan: {string.Join(", ", allowedExtensions)}"
                };
            }

            var filePath = await SaveFileAsync(file, folder);

            return new FileStorageResult
            {
                Success = true,
                FilePath = filePath
            };
        }
        catch (Exception ex)
        {
            return new FileStorageResult
            {
                Success = false,
                ErrorMessage = $"Error al guardar la foto: {ex.Message}"
            };
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

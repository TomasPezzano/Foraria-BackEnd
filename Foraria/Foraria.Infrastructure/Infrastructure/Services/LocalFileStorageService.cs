using ForariaDomain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Foraria.Infrastructure.Infrastructure.Services;

public class LocalFileStorageService : ILocalFileStorageService
{
    private readonly string _baseUploadPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        if (!Directory.Exists(_baseUploadPath))
        {
            Directory.CreateDirectory(_baseUploadPath);
            _logger.LogInformation("Directorio de uploads creado: {Path}", _baseUploadPath);
        }
    }

    public async Task<string> SaveInvoiceFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var safeFileName = Path.GetFileNameWithoutExtension(fileName)
                .Replace(" ", "_")
                .Replace("-", "_");
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{timestamp}_{safeFileName}{extension}";

            var fullPath = Path.Combine(_baseUploadPath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            _logger.LogInformation("Archivo guardado: {FullPath}", fullPath);

            return $"uploads/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar archivo {FileName}", fileName);
            throw new InvalidOperationException($"Error al guardar el archivo: {ex.Message}", ex);
        }
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseUploadPath, Path.GetFileName(filePath));

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Archivo no encontrado: {FilePath}", fullPath);
                throw new FileNotFoundException($"Archivo no encontrado: {filePath}");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer archivo {FilePath}", filePath);
            throw;
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
            _logger.LogError(ex, "Error al guardar foto");
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
            _logger.LogInformation("Carpeta creada: {FolderPath}", folderPath);
        }

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("Archivo guardado: {FullPath}", fullPath);

        return $"uploads/{folder}/{uniqueFileName}";
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        return await Task.Run(() =>
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
                    _logger.LogInformation("Archivo eliminado: {FullPath}", fullPath);
                    return true;
                }

                _logger.LogWarning("Archivo no encontrado para eliminar: {FullPath}", fullPath);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar archivo {FilePath}", filePath);
                return false;
            }
        });
    }

    public Task<string> GetFileUrlAsync(string filePath)
    {
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
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForariaDomain.Services;

public interface ILocalFileStorageService
{
    Task <string> SaveFileAsync(IFormFile file, string directory);
    Task<bool> DeleteFileAsync(string filePath);

    Task <string> GetFileUrlAsync(string filePath);

    Task <bool> ValidateFileAsync(IFormFile file, string[] allowedExtensions, long maxSizeInBytes);

    Task<FileStorageResult> SavePhotoAsync(IFormFile file, string folder, string[] allowedExtensions, long maxFileSize);

    Task<string> SaveInvoiceFileAsync(Stream fileStream, string fileName);

    Task<Stream> GetFileAsync(string filePath);
}

public class FileStorageResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
}


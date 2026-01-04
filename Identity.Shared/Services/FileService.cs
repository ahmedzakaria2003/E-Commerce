using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Identity.Shared.Services;

public class FileService(ILogger<FileService> logger) : IFileService
{
    private readonly ILogger<FileService> _logger = logger;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];

    public async Task<string> SaveImageAsync(IFormFile formFile, string folderPath, string? fileName = null)
    {
        try
        {
            if (formFile == null || formFile.Length == 0)
            {
                _logger.LogWarning("Image file is null or empty");
                return string.Empty;
            }

            var fileExtension = Path.GetExtension(formFile.FileName)?.ToLowerInvariant();
            if (fileExtension == null || !IsValidImageExtension(fileExtension))
            {
                _logger.LogWarning("Invalid image extension: {Extension}", fileExtension);
                return string.Empty;
            }

            if (!await CreateDirectoryIfNotExistAsync(folderPath))
            {
                _logger.LogError("Failed to create directory: {FolderPath}", folderPath);
                return string.Empty;
            }

            fileName = GenerateFileName(fileName, fileExtension);
            var filePath = Path.Combine(folderPath, fileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            await formFile.CopyToAsync(fileStream);

            _logger.LogInformation("Image saved successfully: {FileName}", fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image: {FileName}", fileName);
            return string.Empty;
        }
    }

    public async Task<string> SaveImageFromBase64Async(string base64String, string folderPath, string? fileName = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                _logger.LogWarning("Base64 string is null or empty");
                return string.Empty;
            }

            if (!base64String.Contains(','))
            {
                _logger.LogWarning("Invalid base64 format - missing data URL prefix");
                return string.Empty;
            }

            if (!await CreateDirectoryIfNotExistAsync(folderPath))
            {
                _logger.LogError("Failed to create directory: {FolderPath}", folderPath);
                return string.Empty;
            }

            var splittedData = base64String.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (splittedData.Length < 2)
            {
                _logger.LogWarning("Invalid base64 format");
                return string.Empty;
            }

            var cleanBase64String = splittedData[1];
            var mimeTypeParts = splittedData[0].Split([';', '/'], StringSplitOptions.RemoveEmptyEntries);
            if (mimeTypeParts.Length < 2)
            {
                _logger.LogWarning("Invalid MIME type in base64 string");
                return string.Empty;
            }

            var extension = $".{mimeTypeParts[1]}";
            if (!IsValidImageExtension(extension))
            {
                _logger.LogWarning("Invalid image extension from base64: {Extension}", extension);
                return string.Empty;
            }

            var byteArray = Convert.FromBase64String(cleanBase64String);
            fileName = GenerateFileName(fileName, extension);
            var filePath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(filePath, byteArray);

            _logger.LogInformation("Image from base64 saved successfully: {FileName}", fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image from base64: {FileName}", fileName);
            return string.Empty;
        }
    }

    public async Task<bool> DeleteImageAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("File path is null or empty");
                return false;
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return false;
            }

            // Use FileStream with FileOptions.DeleteOnClose for more reliable deletion
            await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1, FileOptions.DeleteOnClose))
            {
                // File will be deleted when stream is closed
            }

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
            _logger.LogWarning("File is locked, attempting forced deletion: {FilePath}", filePath);
            return await ForceDeleteFileAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return false;
        }
    }

    public bool DeleteImage(string fileName, string directoryPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(directoryPath))
            {
                _logger.LogWarning("File name or directory path is null or empty");
                return false;
            }

            var filePath = Path.Combine(directoryPath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return false;
            }

            // Clear any read-only attribute before deletion
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }

            File.Delete(filePath);
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (IOException ex) when (IsFileLocked(ex))
        {
            _logger.LogWarning("File is locked, attempting GC and retry: {FilePath}", Path.Combine(directoryPath, fileName));
            return ForceDeleteFile(Path.Combine(directoryPath, fileName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", Path.Combine(directoryPath, fileName));
            return false;
        }
    }

    public Task<bool> CreateDirectoryIfNotExistAsync(string folderPath)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                _logger.LogInformation("Directory created: {FolderPath}", folderPath);
            }
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory: {FolderPath}", folderPath);
            return Task.FromResult(false);
        }
    }

    public string SaveImageFile(string base64String, string folderPath, string? fileName = null)
    {
        try
        {
            return SaveImageFromBase64Async(base64String, folderPath, fileName).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SaveImageFile method");
            return string.Empty;
        }
    }

    #region Private Helper Methods

    private static string GenerateFileName(string? fileName, string extension)
    {
        var baseName = string.IsNullOrWhiteSpace(fileName) ? Guid.NewGuid().ToString() : fileName.Trim();
        
        // Remove extension from baseName if it already has one
        var existingExtension = Path.GetExtension(baseName);
        if (!string.IsNullOrEmpty(existingExtension))
        {
            baseName = Path.GetFileNameWithoutExtension(baseName);
        }
        
        return $"{baseName}{extension}";
    }

    private static bool IsValidImageExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return false;
        return AllowedImageExtensions.Contains(extension.ToLowerInvariant());
    }

    private static bool IsFileLocked(IOException ex)
    {
        var errorCode = ex.HResult & 0xFFFF;
        return errorCode == 32 || errorCode == 33; // ERROR_SHARING_VIOLATION or ERROR_LOCK_VIOLATION
    }

    private async Task<bool> ForceDeleteFileAsync(string filePath)
    {
        try
        {
            // Force garbage collection to release any file handles
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            await Task.Delay(100); // Small delay to allow handles to be released

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }

            File.Delete(filePath);
            _logger.LogInformation("File force deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Force delete failed for file: {FilePath}", filePath);
            return false;
        }
    }

    private bool ForceDeleteFile(string filePath)
    {
        try
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(100);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }

            File.Delete(filePath);
            _logger.LogInformation("File force deleted successfully: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Force delete failed for file: {FilePath}", filePath);
            return false;
        }
    }

    #endregion
}

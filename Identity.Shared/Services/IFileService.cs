using Microsoft.AspNetCore.Http;

namespace Identity.Shared.Services;

public interface IFileService
{
    Task<string> SaveImageAsync(IFormFile formFile, string folderPath, string? fileName = null);

    Task<string> SaveImageFromBase64Async(string base64String, string folderPath, string? fileName = null);

    Task<bool> DeleteImageAsync(string filePath);

    bool DeleteImage(string fileName, string directoryPath);

    Task<bool> CreateDirectoryIfNotExistAsync(string folderPath);

    string SaveImageFile(string base64String, string folderPath, string? fileName = null);
}


using Business_Layer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data_Access_Layer.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService( ILogger<FileService> _logger)
        {
            this._logger = _logger;
        }
        public Task DeleteFileAsync(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted File Old Path Was {oldpath}", fullPath);
            }
            return Task.CompletedTask;
        }

        public Task<string> UpdateFileAsync(IFormFile newFile, string folderName, string oldFilePath)
        {
           if (newFile == null) throw new ArgumentNullException(nameof(newFile));
           if (string.IsNullOrEmpty(oldFilePath)) throw new ArgumentNullException(nameof(oldFilePath));
           if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException(nameof(folderName));
           var newFilePath = UploadFileAsync(newFile, folderName);
           var fullOldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldFilePath);
            if (File.Exists(fullOldFilePath))
            {
             File.Delete(fullOldFilePath);
                _logger.LogInformation("Deleted File Old Path Was {oldpath}", fullOldFilePath);
            }
            return newFilePath;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
           _logger.LogInformation($"Uploading file: {file.FileName} to folder: {folderName}"); 
           if (file == null) throw new ArgumentNullException(nameof(file));
           if (string.IsNullOrEmpty(folderName)) throw new ArgumentNullException(nameof(folderName));
           string FolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(FolderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            _logger.LogInformation($"File uploaded successfully: {filePath}");

            return Path.Combine(folderName, fileName);
        }
    }
}

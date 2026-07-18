using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
namespace Business_Layer.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file , string folderName);
        Task<string> UpdateFileAsync(IFormFile newFile, string folderName, string oldFilePath);
        Task DeleteFileAsync(string filePath);
    }
}

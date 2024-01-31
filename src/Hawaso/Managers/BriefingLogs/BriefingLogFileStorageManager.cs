using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Zero.Models
{
    #region BriefingLogBlobStorageManager
    public class BriefingLogBlobStorageManager : IBriefingLogFileStorageManager
    {
        public Task<bool> DeleteAsync(string fileName, string folderPath)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadAsync(string fileName, string folderPath)
        {
            throw new NotImplementedException();
        }

        public string GetFolderPath(string ownerType, string ownerId, string fileType)
        {
            throw new NotImplementedException();
        }

        public string GetFolderPath(string ownerType, long ownerId, string fileType)
        {
            throw new NotImplementedException();
        }

        public string GetFolderPath(string ownerType, int ownerId, string fileType)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadAsync(Stream stream, string fileName, string folderPath, bool overwrite)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
    #region BriefingLogFileStorageManager
    public class BriefingLogFileStorageManager : IBriefingLogFileStorageManager
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _containerName;
        private readonly string _folderPath;

        public BriefingLogFileStorageManager(IWebHostEnvironment environment)
        {
            this._environment = environment;
            _containerName = "files";
            _folderPath = Path.Combine(_environment.WebRootPath, _containerName);
        }

        public async Task<bool> DeleteAsync(string fileName, string folderPath = "BriefingLogs")
        {
            if (File.Exists(Path.Combine(_folderPath, folderPath, fileName)))
            {
                File.Delete(Path.Combine(_folderPath, folderPath, fileName));
            }
            return await Task.FromResult(true);
        }

        public async Task<byte[]> DownloadAsync(string fileName, string folderPath = "BriefingLogs")
        {
            if (File.Exists(Path.Combine(_folderPath, folderPath, fileName)))
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(Path.Combine(_folderPath, folderPath, fileName));
                return fileBytes;
            }
            return null;
        }

        public string GetFolderPath(string ownerType, string ownerId, string fileType)
        {
            throw new NotImplementedException();
        }

        public string GetFolderPath(string ownerType, long ownerId, string fileType)
        {
            throw new NotImplementedException();
        }

        public string GetFolderPath(string ownerType, int ownerId, string fileType)
        {
            throw new NotImplementedException();
        }

        public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = "BriefingLogs", bool overwrite = false)
        {
            await File.WriteAllBytesAsync(Path.Combine(_folderPath, folderPath, fileName), bytes);

            return fileName;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath = "BriefingLogs", bool overwrite = false)
        {
            // 파일명 중복 처리
            fileName = Dul.FileUtility.GetFileNameWithNumbering(Path.Combine(_folderPath, folderPath), fileName);

            using (var fileStream = new FileStream(Path.Combine(_folderPath, folderPath, fileName), FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            return fileName;
        }
    } 
    #endregion
}

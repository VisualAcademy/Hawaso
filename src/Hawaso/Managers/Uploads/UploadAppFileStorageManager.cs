using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using VisualAcademy.Models.Replys;

namespace UploadApp.Managers
{
    public class UploadAppFileStorageManager : IFileStorageManager
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _containerName;
        private readonly string _folderPath;

        public UploadAppFileStorageManager(IWebHostEnvironment environment)
        {
            _environment = environment;
            _containerName = "files";
            _folderPath = Path.Combine(_environment.WebRootPath, _containerName);
        }

        public async Task<bool> DeleteAsync(string fileName, string folderPath)
        {
            var fullPath = Path.Combine(_folderPath, folderPath, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public async Task<byte[]> DownloadAsync(string fileName, string folderPath)
        {
            var fullPath = Path.Combine(_folderPath, folderPath, fileName);

            if (File.Exists(fullPath))
            {
                return await File.ReadAllBytesAsync(fullPath);
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

        public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath, bool overwrite)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException("The file content is empty.", nameof(bytes));
            }

            var directoryPath = Path.Combine(_folderPath, folderPath);
            EnsureDirectoryExists(directoryPath);

            var finalFileName = overwrite
                ? fileName
                : Dul.FileUtility.GetFileNameWithNumbering(directoryPath, fileName);

            var fullPath = Path.Combine(directoryPath, finalFileName);

            await File.WriteAllBytesAsync(fullPath, bytes);

            return finalFileName;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath, bool overwrite)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var directoryPath = Path.Combine(_folderPath, folderPath);
            EnsureDirectoryExists(directoryPath);

            var finalFileName = overwrite
                ? fileName
                : Dul.FileUtility.GetFileNameWithNumbering(directoryPath, fileName);

            var fullPath = Path.Combine(directoryPath, finalFileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }

            return finalFileName;
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }

    #region UploadAppBlobStorageManager
    public class UploadAppBlobStorageManager : IFileStorageManager
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public UploadAppBlobStorageManager(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("BlobConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var storageAccount = configuration["AppKeys:AzureStorageAccount"];
                var storageKey = configuration["AppKeys:AzureStorageAccessKey"];
                var endpointSuffix = configuration["AppKeys:AzureStorageEndpointSuffix"] ?? "core.windows.net";

                connectionString =
                    $"DefaultEndpointsProtocol=https;" +
                    $"AccountName={storageAccount};" +
                    $"AccountKey={storageKey};" +
                    $"EndpointSuffix={endpointSuffix}";
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
            _containerName = "files";
        }

        public async Task<bool> DeleteAsync(string fileName, string folderPath)
        {
            var containerClient = await GetContainerClientAsync();
            var blobName = BuildBlobName(folderPath, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
                return true;
            }

            return false;
        }

        public async Task<byte[]> DownloadAsync(string fileName, string folderPath)
        {
            var containerClient = await GetContainerClientAsync();
            var blobName = BuildBlobName(folderPath, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();

                using (var ms = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(ms);
                    return ms.ToArray();
                }
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

        public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath, bool overwrite)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException("The file content is empty.", nameof(bytes));
            }

            var containerClient = await GetContainerClientAsync();
            var finalFileName = overwrite
                ? fileName
                : await GetUniqueBlobFileNameAsync(containerClient, folderPath, fileName);

            var blobName = BuildBlobName(folderPath, finalFileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var ms = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(ms, overwrite: true);
            }

            return finalFileName;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath, bool overwrite)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return await UploadAsync(ms.ToArray(), fileName, folderPath, overwrite);
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }

        private static string BuildBlobName(string folderPath, string fileName)
        {
            var normalizedFolder = (folderPath ?? string.Empty).Trim().Trim('/', '\\');
            return string.IsNullOrWhiteSpace(normalizedFolder)
                ? fileName
                : $"{normalizedFolder}/{fileName}";
        }

        private async Task<string> GetUniqueBlobFileNameAsync(BlobContainerClient containerClient, string folderPath, string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            int count = 1;

            string candidateFileName = fileName;

            while (await containerClient.GetBlobClient(BuildBlobName(folderPath, candidateFileName)).ExistsAsync())
            {
                candidateFileName = $"{fileNameWithoutExtension}({count++}){extension}";
            }

            return candidateFileName;
        }
    }
    #endregion

    #region UploadAppHybridStorageManager
    /// <summary>
    /// Migration period manager:
    /// - Upload: save to Local + Blob
    /// - Download: Blob first, Local fallback
    /// - Delete: delete from Local + Blob
    /// </summary>
    public class UploadAppHybridStorageManager : IFileStorageManager
    {
        private readonly IWebHostEnvironment _environment;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        private readonly string _folderPath;

        public UploadAppHybridStorageManager(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _environment = environment;
            _folderPath = Path.Combine(_environment.WebRootPath, "files");
            _containerName = "files";

            var connectionString = configuration.GetConnectionString("BlobConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var storageAccount = configuration["AppKeys:AzureStorageAccount"];
                var storageKey = configuration["AppKeys:AzureStorageAccessKey"];
                var endpointSuffix = configuration["AppKeys:AzureStorageEndpointSuffix"] ?? "core.windows.net";

                connectionString =
                    $"DefaultEndpointsProtocol=https;" +
                    $"AccountName={storageAccount};" +
                    $"AccountKey={storageKey};" +
                    $"EndpointSuffix={endpointSuffix}";
            }

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<bool> DeleteAsync(string fileName, string folderPath)
        {
            bool deleted = false;

            var localFilePath = Path.Combine(_folderPath, folderPath, fileName);
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
                deleted = true;
            }

            var containerClient = await GetContainerClientAsync();
            var blobName = BuildBlobName(folderPath, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
                deleted = true;
            }

            return deleted;
        }

        public async Task<byte[]> DownloadAsync(string fileName, string folderPath)
        {
            var containerClient = await GetContainerClientAsync();
            var blobName = BuildBlobName(folderPath, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var response = await blobClient.DownloadAsync();
                using (var ms = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(ms);
                    return ms.ToArray();
                }
            }

            var localFilePath = Path.Combine(_folderPath, folderPath, fileName);
            if (File.Exists(localFilePath))
            {
                return await File.ReadAllBytesAsync(localFilePath);
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

        public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath, bool overwrite)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException("The file content is empty.", nameof(bytes));
            }

            var containerClient = await GetContainerClientAsync();

            var finalFileName = overwrite
                ? fileName
                : await GetUniqueFileNameAcrossLocalAndBlobAsync(containerClient, folderPath, fileName);

            var localDirectory = Path.Combine(_folderPath, folderPath);
            EnsureDirectoryExists(localDirectory);

            var localFilePath = Path.Combine(localDirectory, finalFileName);
            await File.WriteAllBytesAsync(localFilePath, bytes);

            var blobName = BuildBlobName(folderPath, finalFileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var ms = new MemoryStream(bytes))
            {
                await blobClient.UploadAsync(ms, overwrite: true);
            }

            return finalFileName;
        }

        public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath, bool overwrite)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                return await UploadAsync(ms.ToArray(), fileName, folderPath, overwrite);
            }
        }

        private async Task<BlobContainerClient> GetContainerClientAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();
            return containerClient;
        }

        private static string BuildBlobName(string folderPath, string fileName)
        {
            var normalizedFolder = (folderPath ?? string.Empty).Trim().Trim('/', '\\');
            return string.IsNullOrWhiteSpace(normalizedFolder)
                ? fileName
                : $"{normalizedFolder}/{fileName}";
        }

        private async Task<string> GetUniqueFileNameAcrossLocalAndBlobAsync(
            BlobContainerClient containerClient,
            string folderPath,
            string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            string candidate = fileName;
            int count = 1;

            while (await ExistsInLocalOrBlobAsync(containerClient, folderPath, candidate))
            {
                candidate = $"{fileNameWithoutExtension}({count++}){extension}";
            }

            return candidate;
        }

        private async Task<bool> ExistsInLocalOrBlobAsync(
            BlobContainerClient containerClient,
            string folderPath,
            string fileName)
        {
            var localPath = Path.Combine(_folderPath, folderPath, fileName);
            if (File.Exists(localPath))
            {
                return true;
            }

            var blobName = BuildBlobName(folderPath, fileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            return await blobClient.ExistsAsync();
        }

        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
    #endregion
}
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hawaso.Models;

#region MemoFileStorageManager
public class MemoFileStorageManager : IMemoFileStorageManager
{
    private const string moduleName = "Memos";
    private readonly IWebHostEnvironment _environment;
    private readonly string _containerName;
    private readonly string _folderPath;

    public MemoFileStorageManager(IWebHostEnvironment environment)
    {
        _environment = environment;
        _containerName = "files";
        _folderPath = Path.Combine(_environment.WebRootPath, _containerName);
    }

    public async Task<bool> DeleteAsync(string fileName, string folderPath = moduleName)
    {
        var fullPath = Path.Combine(_folderPath, folderPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return await Task.FromResult(true);
        }

        return await Task.FromResult(false);
    }

    public async Task<byte[]> DownloadAsync(string fileName, string folderPath = moduleName)
    {
        var fullPath = Path.Combine(_folderPath, folderPath, fileName);

        if (File.Exists(fullPath))
        {
            return await File.ReadAllBytesAsync(fullPath);
        }

        return null;
    }

    public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = moduleName, bool overwrite = false)
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

    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath = moduleName, bool overwrite = false)
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
#endregion

#region MemoBlobStorageManager
public class MemoBlobStorageManager : IMemoFileStorageManager
{
    private const string ContainerName = "files";
    private const string DefaultFolderPath = "Memos";

    private readonly BlobServiceClient _blobServiceClient;

    public MemoBlobStorageManager(IConfiguration configuration)
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
    }

    public async Task<bool> DeleteAsync(string fileName, string folderPath = DefaultFolderPath)
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

    public async Task<byte[]> DownloadAsync(string fileName, string folderPath = DefaultFolderPath)
    {
        var containerClient = await GetContainerClientAsync();
        var blobName = BuildBlobName(folderPath, fileName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using (var ms = new MemoryStream())
            {
                await downloadInfo.Value.Content.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

        return null;
    }

    public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = DefaultFolderPath, bool overwrite = false)
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

    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath = DefaultFolderPath, bool overwrite = false)
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
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
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

#region MemoHybridStorageManager
/// <summary>
/// 마이그레이션 기간 동안
/// - 업로드: Local + Blob 동시 저장
/// - 다운로드: Blob 우선, 없으면 Local fallback
/// - 삭제: Local + Blob 동시 삭제
/// 를 수행하는 하이브리드 스토리지 매니저
/// </summary>
public class MemoHybridStorageManager : IMemoFileStorageManager
{
    private const string moduleName = "Memos";
    private const string containerName = "files";

    private readonly IWebHostEnvironment _environment;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _folderPath;

    public MemoHybridStorageManager(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _folderPath = Path.Combine(_environment.WebRootPath, containerName);

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

    public async Task<bool> DeleteAsync(string fileName, string folderPath = moduleName)
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

    public async Task<byte[]> DownloadAsync(string fileName, string folderPath = moduleName)
    {
        var containerClient = await GetContainerClientAsync();
        var blobName = BuildBlobName(folderPath, fileName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            var downloadInfo = await blobClient.DownloadAsync();
            using (var ms = new MemoryStream())
            {
                await downloadInfo.Value.Content.CopyToAsync(ms);
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

    public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = moduleName, bool overwrite = false)
    {
        if (bytes == null || bytes.Length == 0)
        {
            throw new ArgumentException("The file content is empty.", nameof(bytes));
        }

        var containerClient = await GetContainerClientAsync();

        var finalFileName = overwrite
            ? fileName
            : await GetUniqueFileNameAcrossLocalAndBlobAsync(containerClient, folderPath, fileName);

        // Local upload
        var localDirectory = Path.Combine(_folderPath, folderPath);
        EnsureDirectoryExists(localDirectory);

        var localFilePath = Path.Combine(localDirectory, finalFileName);
        await File.WriteAllBytesAsync(localFilePath, bytes);

        // Blob upload
        var blobName = BuildBlobName(folderPath, finalFileName);
        var blobClient = containerClient.GetBlobClient(blobName);

        using (var ms = new MemoryStream(bytes))
        {
            await blobClient.UploadAsync(ms, overwrite: true);
        }

        return finalFileName;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath = moduleName, bool overwrite = false)
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
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
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
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using VisualAcademy.Models.Replys;

namespace ReplyApp.Managers;

public class ReplyAppFileStorageManager : IFileStorageManager
{
    private readonly string _folderPath;

    public ReplyAppFileStorageManager(IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        var webRootPath = environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot");

        _folderPath = Path.Combine(webRootPath, "files");
    }

    public Task<bool> DeleteAsync(string fileName, string folderPath)
    {
        var fullPath = Path.Combine(_folderPath, folderPath, fileName);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);

        return Task.FromResult(true);
    }

    public async Task<byte[]> DownloadAsync(
        string fileName,
        string folderPath)
    {
        var fullPath = Path.Combine(_folderPath, folderPath, fileName);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"The requested file was not found: {fileName}",
                fullPath);
        }

        return await File.ReadAllBytesAsync(fullPath);
    }

    public string GetFolderPath(
        string ownerType,
        string ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public string GetFolderPath(
        string ownerType,
        long ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public string GetFolderPath(
        string ownerType,
        int ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public async Task<string> UploadAsync(
        byte[] bytes,
        string fileName,
        string folderPath,
        bool overwrite)
    {
        if (bytes is null || bytes.Length == 0)
        {
            throw new ArgumentException(
                "The file content is empty.",
                nameof(bytes));
        }

        var directoryPath = Path.Combine(_folderPath, folderPath);

        EnsureDirectoryExists(directoryPath);

        var finalFileName = overwrite
            ? fileName
            : Dul.FileUtility.GetFileNameWithNumbering(
                directoryPath,
                fileName);

        var fullPath = Path.Combine(directoryPath, finalFileName);

        await File.WriteAllBytesAsync(fullPath, bytes);

        return finalFileName;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folderPath,
        bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var directoryPath = Path.Combine(_folderPath, folderPath);

        EnsureDirectoryExists(directoryPath);

        var finalFileName = overwrite
            ? fileName
            : Dul.FileUtility.GetFileNameWithNumbering(
                directoryPath,
                fileName);

        var fullPath = Path.Combine(directoryPath, finalFileName);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await stream.CopyToAsync(fileStream);

        return finalFileName;
    }

    private static void EnsureDirectoryExists(string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }
}

#region ReplyAppBlobStorageManager

public class ReplyAppBlobStorageManager : IFileStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public ReplyAppBlobStorageManager(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString =
            GetBlobStorageConnectionString(configuration);

        _blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerName = "files";
    }

    public async Task<bool> DeleteAsync(
        string fileName,
        string folderPath)
    {
        var containerClient =
            await GetContainerClientAsync();

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        var response =
            await blobClient.DeleteIfExistsAsync();

        return response.Value;
    }

    public async Task<byte[]> DownloadAsync(
        string fileName,
        string folderPath)
    {
        var containerClient =
            await GetContainerClientAsync();

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException(
                $"The requested blob was not found: {blobName}",
                blobName);
        }

        var response =
            await blobClient.DownloadAsync();

        await using var memoryStream =
            new MemoryStream();

        await response.Value.Content.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    public string GetFolderPath(
        string ownerType,
        string ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public string GetFolderPath(
        string ownerType,
        long ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public string GetFolderPath(
        string ownerType,
        int ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public async Task<string> UploadAsync(
        byte[] bytes,
        string fileName,
        string folderPath,
        bool overwrite)
    {
        if (bytes is null || bytes.Length == 0)
        {
            throw new ArgumentException(
                "The file content is empty.",
                nameof(bytes));
        }

        var containerClient =
            await GetContainerClientAsync();

        var finalFileName = overwrite
            ? fileName
            : await GetUniqueBlobFileNameAsync(
                containerClient,
                folderPath,
                fileName);

        var blobName =
            BuildBlobName(folderPath, finalFileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        await using var memoryStream =
            new MemoryStream(bytes);

        await blobClient.UploadAsync(
            memoryStream,
            overwrite: true);

        return finalFileName;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folderPath,
        bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(stream);

        await using var memoryStream =
            new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        return await UploadAsync(
            memoryStream.ToArray(),
            fileName,
            folderPath,
            overwrite);
    }

    private async Task<BlobContainerClient>
        GetContainerClientAsync()
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient(
                _containerName);

        await containerClient.CreateIfNotExistsAsync();

        return containerClient;
    }

    private static string BuildBlobName(
        string folderPath,
        string fileName)
    {
        var normalizedFolder = folderPath
            .Trim()
            .Trim('/', '\\');

        return string.IsNullOrWhiteSpace(normalizedFolder)
            ? fileName
            : $"{normalizedFolder}/{fileName}";
    }

    private static string GetBlobStorageConnectionString(
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "BlobConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var storageAccount =
            configuration["AppKeys:AzureStorageAccount"];

        var storageKey =
            configuration["AppKeys:AzureStorageAccessKey"];

        var endpointSuffix =
            configuration["AppKeys:AzureStorageEndpointSuffix"]
            ?? "core.windows.net";

        if (string.IsNullOrWhiteSpace(storageAccount))
        {
            throw new InvalidOperationException(
                "Azure Storage account name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new InvalidOperationException(
                "Azure Storage access key is not configured.");
        }

        return
            $"DefaultEndpointsProtocol=https;" +
            $"AccountName={storageAccount};" +
            $"AccountKey={storageKey};" +
            $"EndpointSuffix={endpointSuffix}";
    }

    private async Task<string> GetUniqueBlobFileNameAsync(
        BlobContainerClient containerClient,
        string folderPath,
        string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        var fileNameWithoutExtension =
            Path.GetFileNameWithoutExtension(fileName);

        var candidateFileName =
            fileName;

        var count = 1;

        while (await containerClient
            .GetBlobClient(
                BuildBlobName(
                    folderPath,
                    candidateFileName))
            .ExistsAsync())
        {
            candidateFileName =
                $"{fileNameWithoutExtension}({count++}){extension}";
        }

        return candidateFileName;
    }
}

#endregion

#region ReplyAppHybridStorageManager

/// <summary>
/// 마이그레이션 기간 동안
/// - 업로드: Local + Blob 동시 저장
/// - 다운로드: Blob 우선, 없으면 Local fallback
/// - 삭제: Local + Blob 동시 삭제
/// 를 수행하는 하이브리드 스토리지 매니저
/// </summary>
public class ReplyAppHybridStorageManager : IFileStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly string _folderPath;

    public ReplyAppHybridStorageManager(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);

        var webRootPath = environment.WebRootPath
            ?? Path.Combine(
                environment.ContentRootPath,
                "wwwroot");

        _folderPath =
            Path.Combine(webRootPath, "files");

        _containerName = "files";

        var connectionString =
            GetBlobStorageConnectionString(configuration);

        _blobServiceClient =
            new BlobServiceClient(connectionString);
    }

    public async Task<bool> DeleteAsync(
        string fileName,
        string folderPath)
    {
        var deleted = false;

        var localFilePath =
            Path.Combine(
                _folderPath,
                folderPath,
                fileName);

        if (File.Exists(localFilePath))
        {
            File.Delete(localFilePath);
            deleted = true;
        }

        var containerClient =
            await GetContainerClientAsync();

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        var blobDeleteResponse =
            await blobClient.DeleteIfExistsAsync();

        if (blobDeleteResponse.Value)
        {
            deleted = true;
        }

        return deleted;
    }

    public async Task<byte[]> DownloadAsync(
        string fileName,
        string folderPath)
    {
        var containerClient =
            await GetContainerClientAsync();

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            var response =
                await blobClient.DownloadAsync();

            await using var memoryStream =
                new MemoryStream();

            await response.Value.Content.CopyToAsync(
                memoryStream);

            return memoryStream.ToArray();
        }

        var localFilePath =
            Path.Combine(
                _folderPath,
                folderPath,
                fileName);

        if (File.Exists(localFilePath))
        {
            return await File.ReadAllBytesAsync(
                localFilePath);
        }

        throw new FileNotFoundException(
            $"The requested file was not found in Blob Storage or local storage: {fileName}",
            localFilePath);
    }

    public string GetFolderPath(
        string ownerType,
        string ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public string GetFolderPath(
        string ownerType,
        long ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public string GetFolderPath(
        string ownerType,
        int ownerId,
        string fileType)
    {
        throw new NotImplementedException();
    }

    public async Task<string> UploadAsync(
        byte[] bytes,
        string fileName,
        string folderPath,
        bool overwrite)
    {
        if (bytes is null || bytes.Length == 0)
        {
            throw new ArgumentException(
                "The file content is empty.",
                nameof(bytes));
        }

        var containerClient =
            await GetContainerClientAsync();

        var finalFileName = overwrite
            ? fileName
            : await GetUniqueFileNameAcrossLocalAndBlobAsync(
                containerClient,
                folderPath,
                fileName);

        var localDirectory =
            Path.Combine(_folderPath, folderPath);

        EnsureDirectoryExists(localDirectory);

        var localFilePath =
            Path.Combine(
                localDirectory,
                finalFileName);

        await File.WriteAllBytesAsync(
            localFilePath,
            bytes);

        var blobName =
            BuildBlobName(
                folderPath,
                finalFileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        await using var memoryStream =
            new MemoryStream(bytes);

        await blobClient.UploadAsync(
            memoryStream,
            overwrite: true);

        return finalFileName;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folderPath,
        bool overwrite)
    {
        ArgumentNullException.ThrowIfNull(stream);

        await using var memoryStream =
            new MemoryStream();

        await stream.CopyToAsync(memoryStream);

        return await UploadAsync(
            memoryStream.ToArray(),
            fileName,
            folderPath,
            overwrite);
    }

    private async Task<BlobContainerClient>
        GetContainerClientAsync()
    {
        var containerClient =
            _blobServiceClient.GetBlobContainerClient(
                _containerName);

        await containerClient.CreateIfNotExistsAsync();

        return containerClient;
    }

    private static string BuildBlobName(
        string folderPath,
        string fileName)
    {
        var normalizedFolder = folderPath
            .Trim()
            .Trim('/', '\\');

        return string.IsNullOrWhiteSpace(normalizedFolder)
            ? fileName
            : $"{normalizedFolder}/{fileName}";
    }

    private static string GetBlobStorageConnectionString(
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "BlobConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var storageAccount =
            configuration["AppKeys:AzureStorageAccount"];

        var storageKey =
            configuration["AppKeys:AzureStorageAccessKey"];

        var endpointSuffix =
            configuration["AppKeys:AzureStorageEndpointSuffix"]
            ?? "core.windows.net";

        if (string.IsNullOrWhiteSpace(storageAccount))
        {
            throw new InvalidOperationException(
                "Azure Storage account name is not configured.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new InvalidOperationException(
                "Azure Storage access key is not configured.");
        }

        return
            $"DefaultEndpointsProtocol=https;" +
            $"AccountName={storageAccount};" +
            $"AccountKey={storageKey};" +
            $"EndpointSuffix={endpointSuffix}";
    }

    private async Task<string>
        GetUniqueFileNameAcrossLocalAndBlobAsync(
            BlobContainerClient containerClient,
            string folderPath,
            string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        var fileNameWithoutExtension =
            Path.GetFileNameWithoutExtension(fileName);

        var candidate =
            fileName;

        var count = 1;

        while (await ExistsInLocalOrBlobAsync(
            containerClient,
            folderPath,
            candidate))
        {
            candidate =
                $"{fileNameWithoutExtension}({count++}){extension}";
        }

        return candidate;
    }

    private async Task<bool> ExistsInLocalOrBlobAsync(
        BlobContainerClient containerClient,
        string folderPath,
        string fileName)
    {
        var localPath =
            Path.Combine(
                _folderPath,
                folderPath,
                fileName);

        if (File.Exists(localPath))
        {
            return true;
        }

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        return await blobClient.ExistsAsync();
    }

    private static void EnsureDirectoryExists(
        string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }
}

#endregion
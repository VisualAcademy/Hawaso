using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using VisualAcademy.Models.Archives;

namespace Hawaso.Managers.Archives;

public class ArchiveFileStorageManager : IArchiveFileStorageManager
{
    private readonly string _containerName;
    private readonly string _folderPath;

    public ArchiveFileStorageManager(IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        if (string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            throw new InvalidOperationException(
                "The web root path is not configured.");
        }

        _containerName = "files";
        _folderPath = Path.Combine(environment.WebRootPath, _containerName);
    }

    public Task<bool> DeleteAsync(
        string fileName,
        string folderPath = "Archives")
    {
        var fullPath = Path.Combine(
            _folderPath,
            folderPath,
            fileName);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);

        return Task.FromResult(true);
    }

    public async Task<byte[]> DownloadAsync(
        string fileName,
        string folderPath = "Archives")
    {
        var fullPath = Path.Combine(
            _folderPath,
            folderPath,
            fileName);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"The requested archive file was not found: '{fileName}'.",
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
        string folderPath = "Archives",
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
        {
            throw new ArgumentException(
                "The file content is empty.",
                nameof(bytes));
        }

        var directoryPath = Path.Combine(
            _folderPath,
            folderPath);

        EnsureDirectoryExists(directoryPath);

        var finalFileName = overwrite
            ? fileName
            : Dul.FileUtility.GetFileNameWithNumbering(
                directoryPath,
                fileName);

        var fullPath = Path.Combine(
            directoryPath,
            finalFileName);

        await File.WriteAllBytesAsync(
            fullPath,
            bytes);

        return finalFileName;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folderPath = "Archives",
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var directoryPath = Path.Combine(
            _folderPath,
            folderPath);

        EnsureDirectoryExists(directoryPath);

        var finalFileName = overwrite
            ? fileName
            : Dul.FileUtility.GetFileNameWithNumbering(
                directoryPath,
                fileName);

        var fullPath = Path.Combine(
            directoryPath,
            finalFileName);

        await using var fileStream = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await stream.CopyToAsync(fileStream);

        return finalFileName;
    }

    private static void EnsureDirectoryExists(
        string directoryPath)
    {
        Directory.CreateDirectory(directoryPath);
    }
}

#region ArchiveBlobStorageManager

public class ArchiveBlobStorageManager : IArchiveFileStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public ArchiveBlobStorageManager(
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
                "AppKeys:AzureStorageAccount is not configured.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new InvalidOperationException(
                "AppKeys:AzureStorageAccessKey is not configured.");
        }

        var connectionString =
            $"DefaultEndpointsProtocol=https;" +
            $"AccountName={storageAccount};" +
            $"AccountKey={storageKey};" +
            $"EndpointSuffix={endpointSuffix}";

        _blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerName = "files";
    }

    public async Task<bool> DeleteAsync(
        string fileName,
        string folderPath = "Archives")
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
        string folderPath = "Archives")
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
                $"The requested archive blob was not found: '{blobName}'.");
        }

        var response =
            await blobClient.DownloadAsync();

        using var memoryStream = new MemoryStream();

        await response.Value.Content.CopyToAsync(
            memoryStream);

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
        string folderPath = "Archives",
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
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
            BuildBlobName(
                folderPath,
                finalFileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        using var memoryStream =
            new MemoryStream(bytes);

        await blobClient.UploadAsync(
            memoryStream,
            overwrite: true);

        return finalFileName;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folderPath = "Archives",
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var memoryStream =
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
        var normalizedFolder =
            (folderPath ?? string.Empty)
            .Trim()
            .Trim('/', '\\');

        return string.IsNullOrWhiteSpace(normalizedFolder)
            ? fileName
            : $"{normalizedFolder}/{fileName}";
    }

    private async Task<string>
        GetUniqueBlobFileNameAsync(
            BlobContainerClient containerClient,
            string folderPath,
            string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        var fileNameWithoutExtension =
            Path.GetFileNameWithoutExtension(fileName);

        var candidateFileName = fileName;
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

#region ArchiveHybridStorageManager

/// <summary>
/// 마이그레이션 기간 동안
/// - 업로드: Local + Blob 동시 저장
/// - 다운로드: Blob 우선, 없으면 Local fallback
/// - 삭제: Local + Blob 동시 삭제
/// 를 수행하는 하이브리드 스토리지 매니저
/// </summary>
public class ArchiveHybridStorageManager : IArchiveFileStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly string _folderPath;

    public ArchiveHybridStorageManager(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(configuration);

        if (string.IsNullOrWhiteSpace(environment.WebRootPath))
        {
            throw new InvalidOperationException(
                "The web root path is not configured.");
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
                "AppKeys:AzureStorageAccount is not configured.");
        }

        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new InvalidOperationException(
                "AppKeys:AzureStorageAccessKey is not configured.");
        }

        var connectionString =
            $"DefaultEndpointsProtocol=https;" +
            $"AccountName={storageAccount};" +
            $"AccountKey={storageKey};" +
            $"EndpointSuffix={endpointSuffix}";

        _blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerName = "files";

        _folderPath = Path.Combine(
            environment.WebRootPath,
            _containerName);
    }

    public async Task<bool> DeleteAsync(
        string fileName,
        string folderPath = "Archives")
    {
        var deleted = false;

        // Local
        var localFilePath = Path.Combine(
            _folderPath,
            folderPath,
            fileName);

        if (File.Exists(localFilePath))
        {
            File.Delete(localFilePath);
            deleted = true;
        }

        // Blob
        var containerClient =
            await GetContainerClientAsync();

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        var response =
            await blobClient.DeleteIfExistsAsync();

        if (response.Value)
        {
            deleted = true;
        }

        return deleted;
    }

    public async Task<byte[]> DownloadAsync(
        string fileName,
        string folderPath = "Archives")
    {
        var containerClient =
            await GetContainerClientAsync();

        var blobName =
            BuildBlobName(folderPath, fileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        // Blob 우선
        if (await blobClient.ExistsAsync())
        {
            var response =
                await blobClient.DownloadAsync();

            using var memoryStream =
                new MemoryStream();

            await response.Value.Content.CopyToAsync(
                memoryStream);

            return memoryStream.ToArray();
        }

        // Local fallback
        var localFilePath = Path.Combine(
            _folderPath,
            folderPath,
            fileName);

        if (File.Exists(localFilePath))
        {
            return await File.ReadAllBytesAsync(
                localFilePath);
        }

        throw new FileNotFoundException(
            $"The requested archive file was not found in Blob or local storage: '{fileName}'.",
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
        string folderPath = "Archives",
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.Length == 0)
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

        // Local 저장
        var localDirectory =
            Path.Combine(
                _folderPath,
                folderPath);

        EnsureDirectoryExists(localDirectory);

        var localFilePath =
            Path.Combine(
                localDirectory,
                finalFileName);

        await File.WriteAllBytesAsync(
            localFilePath,
            bytes);

        // Blob 저장
        var blobName =
            BuildBlobName(
                folderPath,
                finalFileName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        using var memoryStream =
            new MemoryStream(bytes);

        await blobClient.UploadAsync(
            memoryStream,
            overwrite: true);

        return finalFileName;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string folderPath = "Archives",
        bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var memoryStream =
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
        var normalizedFolder =
            (folderPath ?? string.Empty)
            .Trim()
            .Trim('/', '\\');

        return string.IsNullOrWhiteSpace(normalizedFolder)
            ? fileName
            : $"{normalizedFolder}/{fileName}";
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

        var candidate = fileName;
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

    private async Task<bool>
        ExistsInLocalOrBlobAsync(
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
            BuildBlobName(
                folderPath,
                fileName);

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
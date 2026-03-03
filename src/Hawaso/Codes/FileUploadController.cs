using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azunt.Controllers;

/// <summary>
/// Azure Blob Storage에 파일 업로드/다운로드/목록 조회를 제공합니다.
/// - uploadfiles / downloadfiles : 누구나 접근 가능(AllowAnonymous)
/// - listfiles                  : Administrators 롤만 접근 가능
///
/// 연결 정보는 appsettings.json의 AppKeys 섹션을 사용합니다.
/// - AppKeys:AzureStorageAccount
/// - AppKeys:AzureStorageAccessKey
/// - AppKeys:AzureStorageEndpointSuffix
///
/// 동작 개요:
/// - 업로드: wwwroot/files 폴더의 모든 파일(하위 폴더 포함)을 Blob 컨테이너(files)로 업로드(구조 유지)
/// - 다운로드: Blob 컨테이너(files)의 모든 Blob을 wwwroot/files로 다운로드(구조 유지)
///
/// 0byte 이슈 보완:
/// - 업로드: Blob이 0바이트이고 로컬 파일이 0바이트가 아니면 덮어쓰기 업로드
/// - 다운로드: 로컬 파일이 0바이트이고 Blob이 0바이트가 아니면 덮어쓰기 다운로드
///
/// 호환성:
/// - 일부 환경에서 GetBlobsAsync(...)의 cancellationToken 파라미터가 필수(required)로 해석될 수 있어,
///   cancellationToken: CancellationToken.None 을 명시적으로 전달합니다.
/// </summary>
[ApiController]
[Route("[controller]")]
public class FileUploadController(
    IWebHostEnvironment environment,
    IConfiguration configuration) : ControllerBase
{
    private readonly string _containerName = "files";

    /// <summary>
    /// 로컬 wwwroot/files 폴더의 모든 파일을 Azure Blob Storage로 업로드합니다.
    /// 누구나 접근 가능합니다.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("uploadfiles")]
    public async Task<IActionResult> UploadFiles()
    {
        var localPath = Path.Combine(environment.WebRootPath, "files");
        await UploadFilesToBlobAsync(localPath);
        return Ok("Files uploaded successfully.");
    }

    /// <summary>
    /// Azure Blob Storage의 모든 파일을 로컬 wwwroot/files 폴더로 다운로드합니다.
    /// 누구나 접근 가능합니다.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("downloadfiles")]
    public async Task<IActionResult> DownloadFiles()
    {
        var localPath = Path.Combine(environment.WebRootPath, "files");
        await DownloadFilesFromBlobAsync(localPath);
        return Ok("Files downloaded successfully.");
    }

    /// <summary>
    /// Azure Blob Storage의 Documents 폴더(prefix) 아래 파일 목록을 최신 수정일 순으로 반환합니다.
    /// Administrators 롤만 접근 가능합니다.
    /// </summary>
    [Authorize(Roles = "Administrators")]
    [HttpGet("listfiles")]
    public async Task<IActionResult> ListFiles()
    {
        var fileList = await GetFileListFromBlobAsync("Documents");
        return Ok(fileList);
    }

    /// <summary>
    /// appsettings.json(AppKeys) 기반으로 BlobServiceClient를 생성합니다.
    /// AzureStorageEndpointSuffix가 없으면 core.windows.net을 기본값으로 사용합니다.
    /// </summary>
    private BlobServiceClient CreateBlobServiceClient()
    {
        var account = configuration["AppKeys:AzureStorageAccount"];
        var key = configuration["AppKeys:AzureStorageAccessKey"];
        var suffix = configuration["AppKeys:AzureStorageEndpointSuffix"] ?? "core.windows.net";

        var connectionString =
            $"DefaultEndpointsProtocol=https;" +
            $"AccountName={account};" +
            $"AccountKey={key};" +
            $"EndpointSuffix={suffix}";

        return new BlobServiceClient(connectionString);
    }

    /// <summary>
    /// localPath 아래 모든 파일(하위 폴더 포함)을 Blob 컨테이너에 업로드합니다(폴더 구조 유지).
    /// 0byte 이슈 보완:
    /// - 동일 Blob이 존재할 때 Blob이 0바이트 && 로컬 파일이 0보다 크면 overwrite=true로 재업로드합니다.
    /// - 그 외에는 업로드를 생략합니다.
    /// </summary>
    private async Task UploadFilesToBlobAsync(string localPath)
    {
        var blobServiceClient = CreateBlobServiceClient();
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        foreach (var filePath in Directory.GetFiles(localPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(localPath, filePath);
            var blobClient = containerClient.GetBlobClient(relativePath);

            var localFileInfo = new FileInfo(filePath);
            bool overwrite = false;

            if (await blobClient.ExistsAsync())
            {
                var blobProperties = await blobClient.GetPropertiesAsync();
                var blobSize = blobProperties.Value.ContentLength;

                if (blobSize == 0 && localFileInfo.Length > 0)
                {
                    overwrite = true; // Blob이 0바이트인데 로컬이 정상 크기면 덮어쓰기 업로드
                }
                else
                {
                    continue; // 그 외에는 업로드 생략
                }
            }

            using var fileStream = System.IO.File.OpenRead(filePath);
            await blobClient.UploadAsync(fileStream, overwrite: overwrite);
        }
    }

    /// <summary>
    /// Blob 컨테이너의 모든 Blob을 localPath로 다운로드합니다(폴더 구조 유지).
    /// 0byte 이슈 보완:
    /// - 로컬 파일이 존재할 때 로컬이 0바이트 && Blob이 0보다 크면 overwrite=true로 재다운로드합니다.
    /// - 그 외에는 다운로드를 생략합니다.
    /// </summary>
    private async Task DownloadFilesFromBlobAsync(string localPath)
    {
        var blobServiceClient = CreateBlobServiceClient();
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        await foreach (var blobItem in containerClient.GetBlobsAsync(
            BlobTraits.None,
            BlobStates.None,
            prefix: null,
            cancellationToken: CancellationToken.None))
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var downloadPath = Path.Combine(localPath, blobItem.Name);

            var directoryName = Path.GetDirectoryName(downloadPath);
            if (directoryName == null)
                throw new InvalidOperationException("The directory name cannot be null.");

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            bool overwrite = false;

            if (System.IO.File.Exists(downloadPath))
            {
                var localFileInfo = new FileInfo(downloadPath);
                var blobSize = blobItem.Properties.ContentLength ?? 0;

                if (blobSize > 0 && localFileInfo.Length == 0)
                {
                    overwrite = true; // 로컬이 0바이트인데 Blob이 정상 크기면 덮어쓰기 다운로드
                }
                else
                {
                    continue; // 그 외에는 다운로드 생략
                }
            }

            // overwrite=false이고 파일이 존재하면 다운로드하지 않음
            if (!overwrite && System.IO.File.Exists(downloadPath))
                continue;

            var response = await blobClient.DownloadAsync(CancellationToken.None);

            // File.Create는 기존 파일이 있으면 덮어쓰기입니다(여기서는 overwrite=true인 경우만 도달).
            using (var fileStream = System.IO.File.Create(downloadPath))
            {
                await response.Value.Content.CopyToAsync(fileStream, CancellationToken.None);
            }
        }
    }

    /// <summary>
    /// 지정된 prefix(folderName) 아래 Blob 목록을 조회하여 최신 수정일 순으로 반환합니다.
    /// </summary>
    private async Task<List<FileDetails>> GetFileListFromBlobAsync(string folderName)
    {
        var blobServiceClient = CreateBlobServiceClient();
        var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        var fileList = new List<FileDetails>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(
            BlobTraits.None,
            BlobStates.None,
            prefix: folderName,
            cancellationToken: CancellationToken.None))
        {
            fileList.Add(new FileDetails
            {
                FileName = blobItem.Name,
                FileSize = blobItem.Properties.ContentLength ?? 0,
                LastModified = blobItem.Properties.LastModified?.DateTime ?? DateTime.MinValue
            });
        }

        return fileList.OrderByDescending(f => f.LastModified).ToList();
    }
}

/// <summary>
/// 파일의 세부 정보를 나타내는 DTO 입니다.
/// </summary>
public class FileDetails
{
    /// <summary>파일명(Blob 이름)입니다.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>파일 크기(bytes)입니다.</summary>
    public long FileSize { get; set; }

    /// <summary>마지막 수정(생성) 일자입니다.</summary>
    public DateTime LastModified { get; set; }
}
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Hawaso.Models;
using Microsoft.Extensions.Configuration;

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
        this._environment = environment;
        _containerName = "files";
        _folderPath = Path.Combine(_environment.WebRootPath, _containerName);
    }

    public async Task<bool> DeleteAsync(string fileName, string folderPath = moduleName)
    {
        if (File.Exists(Path.Combine(_folderPath, folderPath, fileName)))
        {
            File.Delete(Path.Combine(_folderPath, folderPath, fileName));
        }
        return await Task.FromResult(true);
    }

    public async Task<byte[]> DownloadAsync(string fileName, string folderPath = moduleName)
    {
        if (File.Exists(Path.Combine(_folderPath, folderPath, fileName)))
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(Path.Combine(_folderPath, folderPath, fileName));
            return fileBytes;
        }
        return null;
    }

    public async Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = moduleName, bool overwrite = false)
    {
        await File.WriteAllBytesAsync(Path.Combine(_folderPath, folderPath, fileName), bytes);

        return fileName;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath = moduleName, bool overwrite = false)
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

#region MemoBlobStorageManager
public class MemoBlobStorageManager : IMemoFileStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;
    private const string ContainerName = "files"; // Azure Blob Storage 컨테이너 이름
    private const string DefaultFolderPath = "Memos"; // 기본 폴더 경로를 클래스 레벨 상수로 정의

    public MemoBlobStorageManager(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BlobConnection");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        _containerClient.CreateIfNotExists(); // 컨테이너가 없으면 생성
    }

    public async Task<bool> DeleteAsync(string fileName, string folderPath = DefaultFolderPath)
    {
        var blobClient = _containerClient.GetBlobClient(Path.Combine(folderPath, fileName));
        return await blobClient.DeleteIfExistsAsync();
    }

    public async Task<byte[]> DownloadAsync(string fileName, string folderPath = DefaultFolderPath)
    {
        var blobClient = _containerClient.GetBlobClient(Path.Combine(folderPath, fileName));
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
        var blobClient = _containerClient.GetBlobClient(Path.Combine(folderPath, fileName));
        using (var ms = new MemoryStream(bytes))
        {
            await blobClient.UploadAsync(ms, overwrite);
        }
        return blobClient.Uri.ToString(); // 업로드된 파일의 URI 반환
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string folderPath = DefaultFolderPath, bool overwrite = false)
    {
        var blobClient = _containerClient.GetBlobClient(Path.Combine(folderPath, fileName));
        await blobClient.UploadAsync(stream, overwrite);
        return blobClient.Uri.ToString(); // 업로드된 파일의 URI 반환
    }
}
#endregion

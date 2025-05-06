using Azunt.FileManagement;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net; // URL 디코딩 및 인코딩을 위한 네임스페이스
using System.Threading.Tasks;

namespace Azunt.Web.Components.Pages.FilesPages.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IConfiguration config)
        {
            var connStr = config["AzureBlobStorage:Default:ConnectionString"];
            var containerName = config["AzureBlobStorage:Default:ContainerName"];

            _containerClient = new BlobContainerClient(connStr, containerName);
            _containerClient.CreateIfNotExists();
        }

        // 파일 업로드 시 URL 인코딩된 파일명 처리
        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            // URL 인코딩 처리
            string encodedFileName = WebUtility.UrlEncode(fileName);

            // 파일명 중복 방지 처리
            string safeFileName = await GetUniqueFileNameAsync(encodedFileName);
            var blobClient = _containerClient.GetBlobClient(safeFileName);

            // 파일 업로드
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobClient.Uri.ToString(); // 전체 URL 반환
        }

        // 파일명을 안전하게 고유하게 만듦 (중복 방지)
        private async Task<string> GetUniqueFileNameAsync(string fileName)
        {
            string baseName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string newFileName = fileName;
            int count = 1;

            // Blob Storage에서 파일이 이미 존재하는지 체크
            while (await _containerClient.GetBlobClient(newFileName).ExistsAsync())
            {
                newFileName = $"{baseName}({count}){extension}";
                count++;
            }

            return newFileName;
        }

        // 파일 다운로드 시 URL 디코딩된 파일명 처리
        public async Task<Stream> DownloadAsync(string fileName)
        {
            // URL 디코딩 처리
            string decodedFileName = WebUtility.UrlDecode(fileName);

            var blobClient = _containerClient.GetBlobClient(decodedFileName);

            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException($"FileEntity not found: {fileName}");

            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        // 파일 삭제 시 URL 디코딩된 파일명 처리
        public Task DeleteAsync(string fileName)
        {
            // URL 디코딩 처리
            string decodedFileName = WebUtility.UrlDecode(fileName);

            var blobClient = _containerClient.GetBlobClient(decodedFileName);
            return blobClient.DeleteIfExistsAsync();
        }
    }
}

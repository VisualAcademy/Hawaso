using System.IO;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    /// <summary>
    /// 파일(BLOB) 업로드 및 다운로드에 대한 메서드 시그니처 정리
    /// </summary>
    public interface IMemoFileStorageManager
    {
        /// <summary>
        /// File(Blob) Upload with byte[]
        /// </summary>
        /// <returns>New FileName</returns>
        Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = "Memos", bool overwrite = false);

        /// <summary>
        /// File(Blob) Upload with Stream
        /// </summary>
        Task<string> UploadAsync(Stream stream, string fileName, string folderPath = "Memos", bool overwrite = false);

        /// <summary>
        /// File(Blob) Download
        /// </summary>
        /// <returns>File(Blob)</returns>
        Task<byte[]> DownloadAsync(string fileName, string folderPath = "Memos");

        /// <summary>
        /// File(Blob) Delete
        /// </summary>
        /// <returns>true or false</returns>
        Task<bool> DeleteAsync(string fileName, string folderPath = "Memos");
    }
}

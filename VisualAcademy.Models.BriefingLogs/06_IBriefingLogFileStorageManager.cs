namespace Zero.Models
{
    public interface IBriefingLogFileStorageManager
    {
        /// <summary>
        /// File(Blob) Upload
        /// </summary>
        /// <returns>New FileName</returns>
        Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = "BriefingLogs", bool overwrite = false);
        Task<string> UploadAsync(Stream stream, string fileName, string folderPath = "BriefingLogs", bool overwrite = false);

        /// <summary>
        /// File(Blob) Download
        /// </summary>
        /// <returns>File(Blob)</returns>
        Task<byte[]> DownloadAsync(string fileName, string folderPath = "BriefingLogs");

        /// <summary>
        /// File(Blob) Delete
        /// </summary>
        /// <returns>true or false</returns>
        Task<bool> DeleteAsync(string fileName, string folderPath = "BriefingLogs");

        /// <summary>
        /// Get Sub Folder with string
        /// </summary>
        string GetFolderPath(string ownerType, string ownerId, string fileType);

        /// <summary>
        /// Get Sub Folder with long
        /// </summary>
        string GetFolderPath(string ownerType, long ownerId, string fileType);

        /// <summary>
        /// Get Sub Folder with int
        /// </summary>
        string GetFolderPath(string ownerType, int ownerId, string fileType);
    }
}

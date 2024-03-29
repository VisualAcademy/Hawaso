﻿using System.IO;
using System.Threading.Tasks;

namespace Hawaso.Models
{
    public interface IPurgeFileStorageManager
    {
        /// <summary>
        /// File(Blob) Upload
        /// </summary>
        /// <returns>New FileName</returns>
        Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath = "Purges", bool overwrite = false);
        Task<string> UploadAsync(Stream stream, string fileName, string folderPath = "Purges", bool overwrite = false);

        /// <summary>
        /// File(Blob) Download
        /// </summary>
        /// <returns>File(Blob)</returns>
        Task<byte[]> DownloadAsync(string fileName, string folderPath = "Purges");

        /// <summary>
        /// File(Blob) Delete
        /// </summary>
        /// <returns>true or false</returns>
        Task<bool> DeleteAsync(string fileName, string folderPath = "Purges");

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

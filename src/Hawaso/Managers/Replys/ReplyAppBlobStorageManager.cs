﻿using VisualAcademy.Models.Replys;

namespace ReplyApp.Managers;

public class ReplyAppBlobStorageManager : IFileStorageManager
{
    public Task<bool> DeleteAsync(string fileName, string folderPath)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> DownloadAsync(string fileName, string folderPath)
    {
        throw new NotImplementedException();
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

    public Task<string> UploadAsync(byte[] bytes, string fileName, string folderPath, bool overwrite)
    {
        throw new NotImplementedException();
    }

    public Task<string> UploadAsync(Stream stream, string fileName, string folderPath, bool overwrite)
    {
        throw new NotImplementedException();
    }
}

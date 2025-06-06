using Azunt.NoteManagement;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Azunt.Web.Components.Pages.Notes.Services
{
    /// <summary>
    /// 데모 환경용 스토리지 서비스 구현체.
    /// 업로드, 다운로드, 삭제 기능은 실제로 동작하지 않음.
    /// </summary>
    public class NoOpNoteStorageService : INoteStorageService
    {
        public Task<string> UploadAsync(Stream stream, string fileName)
        {
            // 파일 업로드는 무시하고 원래 이름 그대로 반환
            return Task.FromResult(fileName ?? "demo.txt");
        }

        public Task<Stream> DownloadAsync(string fileName)
        {
            // 다운로드 요청 시 빈 메모리 스트림 반환
            var dummyStream = new MemoryStream();
            var writer = new StreamWriter(dummyStream);
            writer.Write($"This is a demo file: {fileName}");
            writer.Flush();
            dummyStream.Position = 0;
            return Task.FromResult<Stream>(dummyStream);
        }

        public Task DeleteAsync(string fileName)
        {
            // 아무 작업도 하지 않음
            return Task.CompletedTask;
        }
    }
}

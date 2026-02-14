namespace BlazorInputFile;

// This is public only because it's used in a JSInterop method signature,
// but otherwise is intended as internal
public class FileListEntryImpl : IFileListEntry
{
    internal InputFile? Owner { get; set; }

    private Stream? _stream;

    public event EventHandler? OnDataRead;

    public int Id { get; set; }

    public DateTime LastModified { get; set; }

    public string Name { get; set; } = string.Empty;

    public long Size { get; set; }

    public string Type { get; set; } = string.Empty;

    public Stream Data
    {
        get
        {
            if (Owner is null)
                throw new InvalidOperationException("Owner is not initialized.");

            _stream ??= Owner.OpenFileStream(this);
            return _stream;
        }
    }

    internal void RaiseOnDataRead()
    {
        OnDataRead?.Invoke(this, EventArgs.Empty);
    }
}

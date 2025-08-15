namespace Academy.Shared.Storage.Models
{
    public record Asset(
        Guid Id,
        string Path,
        string FileName,
        long FileLength,
        string FileContentType
    );
}

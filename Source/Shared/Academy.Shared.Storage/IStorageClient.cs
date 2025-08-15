using Academy.Shared.Storage.Models;

using Microsoft.AspNetCore.Http;

namespace Academy.Shared.Storage
{
    public interface IStorageClient
    {
        Task<Asset?> UploadAssetAsync(IFormFile file);
        Task<Asset?> UploadAssetAsync(Stream stream, string fileName, string fileContentType, long fileLength);
    }
}

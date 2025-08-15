using Academy.Shared.Storage.Models;

using Microsoft.AspNetCore.Http;

namespace Academy.Shared.Storage
{
    public interface IStorageClient
    {
        Task<Asset?> UploadAssetAsync(IFormFile file, string folder);
        Task<Asset?> UploadAssetAsync(Stream stream, string folder, string fileName, string fileContentType, long fileLength);
    }
}

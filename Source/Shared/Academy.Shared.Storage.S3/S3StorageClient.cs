using Academy.Shared.Storage.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Minio;
using Minio.DataModel.Args;

namespace Academy.Shared.Storage.S3
{
    public class S3StorageClient : IStorageClient
    {
        private readonly ILogger<S3StorageClient> _logger;
        private readonly string _apiUrl = string.Empty;
        private readonly string _accessKey = string.Empty;
        private readonly string _secretKey = string.Empty;
        private readonly bool _useSSL = false;
        private readonly string _bucket = string.Empty;
        private readonly IMinioClient _client;

        public S3StorageClient(ILogger<S3StorageClient> logger,
                               string apiUrl,
                               string accessKey,
                               string secretKey,
                               bool useSSL,
                               string bucket)
        {
            _logger = logger;
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _accessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey));
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            _useSSL = useSSL;
            _bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));

            IMinioClient clientBuilder = new MinioClient()
                   .WithEndpoint(_apiUrl)
                   .WithCredentials(_accessKey, _secretKey);

            if (_useSSL)
            {
                clientBuilder = clientBuilder.WithSSL();
            }

            _client = clientBuilder.Build();
        }


        private static async Task EnsureBucketExistsAsync(IMinioClient minio, string bucket)
        {
            bool exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
            if (!exists)
            {
                await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
            }
        }


        public async Task<Asset?> UploadAssetAsync(Stream stream, string folder, string fileName, string fileContentType, long fileLength)
        {
            await EnsureBucketExistsAsync(_client, _bucket);

            Guid id = Guid.NewGuid();
            string objectName = (!string.IsNullOrEmpty(folder) ? $"{folder}/" :"") + $"{id:N}{Path.GetExtension(fileName)}";
            string contentType = string.IsNullOrWhiteSpace(fileContentType) ? "application/octet-stream" : fileContentType;

            PutObjectArgs putArgs = new PutObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(fileLength)
                    .WithContentType(contentType);

            try
            {
                var resp = await _client.PutObjectAsync(putArgs);
                if (resp != null)
                {
                    if(resp.ResponseStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation("Successfully uploaded asset {FileName} to S3", fileName);
                    }
                    else
                    {
                        _logger.LogWarning("Upload failed with status code {StatusCode} for asset {FileName}", resp.ResponseStatusCode, fileName);

                        return null;
                    }                    
                }
                else
                {
                    _logger.LogError("Upload response was null for asset {FileName}", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload asset {FileName} to S3", fileName);
                return null;
            }

            return new Asset(
                id,
                $"{_bucket}/{objectName}",
                fileName,
                fileLength,
                contentType
            );
        }

        public Task<Asset?> UploadAssetAsync(IFormFile file, string folder)
        {
            return UploadAssetAsync(
                file.OpenReadStream(),
                folder,
                file.FileName,
                file.ContentType,
                file.Length
            );
        }
    }
}

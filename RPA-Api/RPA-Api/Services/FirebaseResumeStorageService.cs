using Google.Cloud.Storage.V1;
using RPA_Api.IService;

namespace RPA_Api.Services
{
    public class FirebaseResumeStorageService : IResumeStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly IConfiguration _config;

        public FirebaseResumeStorageService(StorageClient storageClient, IConfiguration config)
        {
            _storageClient = storageClient;
            _config = config;
        }

        public async Task<string> UploadResumeAsync(
            string userId,
            string fileName,
            string contentType,
            Stream content,
            CancellationToken ct)
        {
            var bucket = _config["Firebase:StorageBucket"]
                ?? throw new InvalidOperationException("Firebase:StorageBucket is missing.");

            var safeFileName = Path.GetFileName(fileName);
            var objectName = $"resumes/{userId}/{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}_{safeFileName}";

            await _storageClient.UploadObjectAsync(
                bucket: bucket,
                objectName: objectName,
                contentType: contentType,
                source: content,
                options: new UploadObjectOptions(),
                cancellationToken: ct);

            return objectName; // store path in Firestore (best practice)
        }
    }

}

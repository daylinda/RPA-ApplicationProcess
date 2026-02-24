using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using RPA_Api.IService;
using RPA_Api.Model;

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

        private static bool IsResumeFile(string objectName)
        {
            var lower = objectName.ToLowerInvariant();
            return lower.EndsWith(".pdf") || lower.EndsWith(".doc") || lower.EndsWith(".docx");
        }

        public async Task<ResumeCollection> GetResumesByUserId(string userId)
        {
            var bucketName = _config["Firebase:StorageBucket"]
                ?? throw new InvalidOperationException("Firebase:StorageBucket is missing.");

            var prefix = $"resumes/{userId}/";

            var listResumes = new List<ResumeRecord>();

            await foreach (var obj in _storageClient.ListObjectsAsync(bucketName, prefix))
            {
                if (obj.Name.EndsWith("/")) continue;
                if (!IsResumeFile(obj.Name)) continue;

                listResumes.Add(new ResumeRecord
                {
                    UserId = userId,
                    FileName = obj.Name.Split('/').Last(),
                    StoragePath = obj.Name,
                    ContentType = obj.ContentType,
                    SizeBytes = (long?)obj.Size ?? 0,
                    CreatedUtc = obj.TimeCreated?.ToUniversalTime() ?? DateTime.UtcNow
                });
            }

            return new ResumeCollection { resumeRecords = listResumes };
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
            var objectName = $"resumes/{userId}/{Guid.NewGuid()}_{safeFileName}";

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

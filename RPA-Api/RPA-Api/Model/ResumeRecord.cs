using Google.Cloud.Firestore;

namespace RPA_Api.Model
{

    [FirestoreData]
    public class ResumeRecord
    {
        [FirestoreProperty]
        public string ResumeId { get; set; } = default!;
        [FirestoreProperty]
        public string UserId { get; set; } = default!;
        [FirestoreProperty]
        public string FileName { get; set; } = default!;
        [FirestoreProperty]
        public string ContentType { get; set; } = default!;
        [FirestoreProperty]
        public long SizeBytes { get; set; }
        [FirestoreProperty]
        public string StoragePath { get; set; } = default!;
        [FirestoreProperty]
        public DateTimeOffset UploadedAt { get; set; }
        [FirestoreProperty]
        public string Status { get; set; } = "Uploaded"; // Uploaded | Analysing | Analysed | Failed
        [FirestoreProperty]
        public DateTime CreatedUtc { get; internal set; }= default!;
    }
}

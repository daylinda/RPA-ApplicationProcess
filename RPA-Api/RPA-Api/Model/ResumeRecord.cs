namespace RPA_Api.Model
{
    public class ResumeRecord
    {
        public string ResumeId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string FileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public long SizeBytes { get; set; }
        public string StoragePath { get; set; } = default!;
        public DateTimeOffset UploadedAt { get; set; }
        public string Status { get; set; } = "Uploaded"; // Uploaded | Analysing | Analysed | Failed
    }
}

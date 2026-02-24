namespace RPA_Api.Model
{
    public class ResumeUploadRequest
    {
        public IFormFile File { get; set; }
        public string UserId { get; set; }
        public CancellationToken CT { get; set; }
    }
}

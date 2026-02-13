namespace RPA_Api.Model
{
    public class ResumeUploadRequest
    {
        public IFormFile File;
        public string UserId;
        public CancellationToken CT;
    }
}

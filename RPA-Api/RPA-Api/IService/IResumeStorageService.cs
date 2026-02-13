namespace RPA_Api.IService
{
    public interface IResumeStorageService
    {
        Task<string> UploadResumeAsync(
        string userId,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken ct);
    }
}

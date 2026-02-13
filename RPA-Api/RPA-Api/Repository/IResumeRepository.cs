using RPA_Api.Model;

namespace RPA_Api.Repository
{
    public interface IResumeRepository
    {
        Task CreateAsync(ResumeRecord record, CancellationToken ct);
    }

}

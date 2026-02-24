

using RPA_Api.Model;

namespace RPA_Api.Repository
{
    public interface IUserRepository
    {
        Task CreateAsync(UserRecord record);
    }
}

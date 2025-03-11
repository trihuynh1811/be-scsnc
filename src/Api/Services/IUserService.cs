using Infrastructure.Model;

namespace Api.Services
{
    public interface IUserService
    {
        public List<User> GetAllUsers();

        public User GetUserByEmail(string email);

        bool IsAuthenticated(string? password, string? passwordHash);

    }
}

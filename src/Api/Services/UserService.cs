using Infrastructure.Data;
using Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class UserService: IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.Include(u => u.Token).ToList();
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.Include(u => u.Token).ToList().Find(u => u.Email == email);
        }

        public bool IsAuthenticated(string? password, string? passwordHash)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(passwordHash);

            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}

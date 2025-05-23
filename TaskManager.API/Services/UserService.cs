using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Services
{
    public class UserService
    {
        private readonly Infrastructure.AppDbContext _context;
        public UserService(Infrastructure.AppDbContext context) => _context = context;

        public async Task<bool> UserExists(string login) =>
            await _context.Users.AnyAsync(u => u.Login == login);
        public async Task<List<User>> GetAllUsers() =>
            await _context.Users.Select(x => new User { Id = x.Id, Name = x.Name, Login = x.Login }).ToListAsync();

        public async Task<User> GetCurrentUser(string login)
        {
            var user = await _context.Users.FirstAsync(x => x.Login == login);
            user.Password = string.Empty;
            return user;
        }

        public async Task Register(RegisterUserCommand cmd)
        {
            var user = new User { Login = cmd.Login, Password = cmd.Password, Name = cmd.Name };
            user.Id = Guid.NewGuid();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AuthenticatedUser?> Login(LoginUserCommand cmd)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == cmd.Login && u.Password == cmd.Password);
            return user is null ? null : new AuthenticatedUser(user.Login, user.Name);
        }

        public async Task<bool> Validate(string login, string password) =>
            await _context.Users.AnyAsync(u => u.Login == login && u.Password == password);
    }
    public record RegisterUserCommand(string Login, string Password, string Name);
    public record LoginUserCommand(string Login, string Password);
    public record AuthenticatedUser(string Login, string Name);
}

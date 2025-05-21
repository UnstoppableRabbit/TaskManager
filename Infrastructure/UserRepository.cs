using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class UserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context) => _context = context;

        public Task<User?> GetByIdAsync(Guid id) =>
            _context.Users.FirstOrDefaultAsync(t => t.Id == id);
        public Task<User?> GetByLoginAsync(string login) =>
          _context.Users.FirstOrDefaultAsync(t => t.Login == login);

        public Task<List<User>> GetAllAsync() =>
            _context.Users.ToListAsync();

        public Task AddAsync(User user)
        {
            _context.Users.Add(user);
            return _context.SaveChangesAsync();
        }

        public Task UpdateAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(t => t.Id == id);
            if (user is null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<User> Query() => _context.Users.AsQueryable();
    }
}

using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class TaskRepository
    {
        private readonly AppDbContext _context;
        public TaskRepository(AppDbContext context) => _context = context;

        public Task<TaskItem?> GetByIdAsync(Guid id) =>
            _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        public Task<List<TaskItem>> GetAllAsync() =>
            _context.Tasks.ToListAsync();

        public Task AddAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            return _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Guid taskId, TaskItem task)
        {
            var existingTask = _context.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (existingTask is null) throw new Exception("Task by this id not found");
            existingTask.Comments = task.Comments;
            existingTask.ActualDuration = task.ActualDuration;
            existingTask.Status = task.Status;
            return _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.CreatedByUserId == userId);
            if (task is null) return false;
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<TaskItem> Query() => _context.Tasks.AsQueryable();
    }
}

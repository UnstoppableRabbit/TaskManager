using Domain.Enums;
using Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class TaskRepository
    {
        private readonly AppDbContext _context;
        public TaskRepository(AppDbContext context) => _context = context;

        public Task<TaskItem?> GetByIdAsync(Guid id) =>
            _context.Tasks.Include(x => x.Comments).FirstOrDefaultAsync(t => t.Id == id);

        public Task<List<TaskItem>> GetAllAsync() =>
            _context.Tasks.Include(x => x.Comments).ToListAsync();

        public Task AddAsync(TaskItem task)
        {
            _context.Tasks.Add(task);
            _context.Comments.AddRange(task.Comments);
            return _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid taskId, TaskItem task, Guid userId)
        {
            var existingTask = _context.Tasks
                 .Include(t => t.Comments)
                 .FirstOrDefault(t => t.Id == taskId && t.CreatedByUserId == userId);

            if (existingTask is null)
                throw new Exception("Task by this id not found");

            existingTask.ActualDuration = task.ActualDuration;
            existingTask.Status = task.Status;

            if (existingTask.Status == StatusTask.Done)
            {
                existingTask.ClosedAt = DateTime.UtcNow;
            }

            foreach (var newComment in task.Comments)
            {
                if (!existingTask.Comments.Any(c => c.Id == newComment.Id))
                {
                    newComment.TaskItemId = taskId;
                    newComment.UserId = userId;
                    existingTask.Comments.Add(newComment);
                    _context.Comments.Add(newComment);
                }
            }

            await _context.SaveChangesAsync();
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

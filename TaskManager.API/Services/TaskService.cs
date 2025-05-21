using Domain.Enums;
using Domain.Model;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepo;
        public TaskService(TaskRepository taskRepo) => _taskRepo = taskRepo;

        public async Task<TaskItem> Create(CreateTaskCommand cmd, Guid userId)
        {
            try {
                var task = new TaskItem
                {
                    Title = cmd.Title,
                    Description = cmd.Description,
                    CreatedAt = cmd.DueAt,
                    CreatedByUserId = userId
                };
                await _taskRepo.AddAsync(task);
                return task;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating task", ex);
            }
        }

        public async Task<bool> Delete(Guid id, Guid userId)
        {
            try
            {
                var task = await _taskRepo.DeleteAsync(id, userId);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting task", ex);
            }
        }

        public async Task<TaskItem?> Update(Guid id, UpdateTaskCommand cmd, Guid userId)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task is null) return null;

            if (Enum.IsDefined(typeof(StatusTask), cmd.Status)) task.Status = (StatusTask)cmd.Status;
            if (cmd.ActualTime is not null) task.ActualTime = cmd.ActualTime;

            if (cmd.Status == "Завершена")
            {
                task.ClosedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<IEnumerable<TaskItem>> GetFiltered(string? status, string? createdBy,
            DateTime? from, DateTime? to, string? sortBy)
        {
            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(t => t.CreatedBy == createdBy);

            if (from is not null)
                query = query.Where(t => t.CreatedAt >= from);

            if (to is not null)
                query = query.Where(t => t.CreatedAt <= to);

            query = sortBy switch
            {
                "date" => query.OrderBy(t => t.CreatedAt),
                "user" => query.OrderBy(t => t.CreatedBy),
                "status" => query.OrderBy(t => t.Status),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            return await query.ToListAsync();
        }
    }
}

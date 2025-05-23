using Domain.Enums;
using Domain.Model;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.API.Services
{
    public class TaskService
    {
        private readonly TaskRepository _taskRepo;
        private readonly UserRepository _userRepo;
        public TaskService(TaskRepository taskRepo, UserRepository userRepo) 
        { 
            _taskRepo = taskRepo;
            _userRepo = userRepo;
        }

        public async Task<TaskItem> Create(TaskItem task, string userLogin)
        {
            try 
            {
                var user = await _userRepo.GetByLoginAsync(userLogin);
                if (user is null)
                    throw new Exception("User not found");
                task.CreatedByUserId = user.Id;
                foreach (var com in task.Comments)
                {
                    com.UserId = user.Id;
                    com.CreatedAt = DateTime.UtcNow;
                }
                await _taskRepo.AddAsync(task);
                return task;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating task", ex);
            }
        }

        public async Task<bool> Delete(Guid id, string userLogin)
        {
            try
            {
                var user = await _userRepo.GetByLoginAsync(userLogin);
                if (user is null) 
                    throw new Exception("User not found");
                var task = await _taskRepo.DeleteAsync(id, user.Id);
                return task;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting task", ex);
            }
        }

        public async Task<TaskItem?> Update(Guid id, TaskItem updatedTask, string userLogin)
        {
            var user = await _userRepo.GetByLoginAsync(userLogin);
            if (user is null)
                throw new Exception("User not found");
            await _taskRepo.UpdateAsync(id, updatedTask, user.Id);
            return updatedTask;
        }
        public async Task<IEnumerable<TaskItem>> GetAllTasks()
        {
            var tasks = await _taskRepo.GetAllAsync();
            return tasks;
        }
        public async Task<IEnumerable<TaskItem>> GetFiltered(StatusTask? status, Guid? createdBy,
        DateTime? from, DateTime? to, string? sortBy)
        {
            var query = _taskRepo.Query();

            if (status != null)
                query = query.Where(t => t.Status == status);

            if (createdBy != null)
                query = query.Where(t => t.CreatedByUserId == createdBy);

            if (from is not null)
                query = query.Where(t => t.CreatedAt >= from);

            if (to is not null)
                query = query.Where(t => t.CreatedAt <= to);

            query = sortBy switch
            {
                "date" => query.OrderBy(t => t.CreatedAt),
                "user" => query.OrderBy(t => t.CreatedByUserId),
                "status" => query.OrderBy(t => t.Status),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };

            return await query.ToListAsync();
        }
    }
}

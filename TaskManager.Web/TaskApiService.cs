using Domain.Enums;
using Domain.Model;
using System.Net.Http.Json;

namespace TaskManager.Web
{
    public class TaskApiService
    {
        private readonly HttpClient _http;

        public TaskApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IEnumerable<TaskItem>?> GetTasksAsync(StatusTask? status = null, Guid? createdBy = null, DateTime? from = null, DateTime? to = null, string? sortBy = null)
        {
            var query = new List<string>();
            if (status != null) query.Add($"status={status}");
            if (createdBy != null) query.Add($"createdBy={createdBy}");
            if (from != null) query.Add($"from={from.Value:O}");
            if (to != null) query.Add($"to={to.Value:O}");
            if (sortBy != null) query.Add($"sortBy={sortBy}");

            var url = "/tasks";
            if (query.Any()) url += "?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<IEnumerable<TaskItem>>(url);
        }

        public async Task<TaskItem?> CreateTaskAsync(TaskItem task)
        {
            var result = await _http.PostAsJsonAsync("/tasks", task);
            return await result.Content.ReadFromJsonAsync<TaskItem>();
        }

        public async Task<bool> DeleteTaskAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"/tasks/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<TaskItem?> UpdateTaskAsync(Guid id, TaskItem cmd)
        {
            var result = await _http.PutAsJsonAsync($"/tasks/{id}", cmd);
            return result.IsSuccessStatusCode ? await result.Content.ReadFromJsonAsync<TaskItem>() : null;
        }
    }
}

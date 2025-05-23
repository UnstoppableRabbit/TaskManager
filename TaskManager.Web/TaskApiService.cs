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

        public async Task<IEnumerable<TaskItem>?> GetTasksAsync()
        {
            var url = "/tasks";
            
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

        public async Task<TaskItem?> UpdateTaskAsync(Guid id, TaskItem task)
        {
            var result = await _http.PutAsJsonAsync($"/tasks/{id}", task);
            return result.IsSuccessStatusCode ? await result.Content.ReadFromJsonAsync<TaskItem>() : null;
        }
    }
}

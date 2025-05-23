using Domain.Model;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManager.API.Services;

namespace TaskManager.API.Extentions
{
    public static class TaskEndpoints
    {
        public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/tasks", [Authorize] async (TaskItem task, TaskService taskService, ClaimsPrincipal user) =>
            {
                var resultTask = await taskService.Create(task, user.Identity!.Name!);
                return Results.Ok(resultTask);
            });

            app.MapDelete("/tasks/{id}", [Authorize] async (Guid id, TaskService taskService, ClaimsPrincipal user) =>
            {
                var result = await taskService.Delete(id, user.Identity!.Name!);
                return result ? Results.Ok() : Results.NotFound();
            });

            app.MapPut("/tasks/{id}", [Authorize] async (Guid id, TaskItem task, TaskService taskService, ClaimsPrincipal user) =>
            {
                var result = await taskService.Update(id, task, user.Identity!.Name!);
                return result is null ? Results.NotFound() : Results.Ok(result);
            });

            app.MapGet("/tasks", [Authorize] async (TaskService taskService) =>
            {
                var tasks = await taskService.GetAllTasks();
                return Results.Ok(tasks);
            });
        }
    }
}

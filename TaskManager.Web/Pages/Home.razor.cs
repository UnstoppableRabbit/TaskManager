using Domain.Enums;
using Domain.Model;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using TaskManager.Web.Components;
using TaskManager.Web.Services;
using Microsoft.AspNetCore.Authorization;

namespace TaskManager.Web.Pages
{
    [Authorize]
    public partial class Home
    {
        [Inject] private TaskApiService taskApiService { get; set; } = default!;
        [Inject] private UserApiService userApiService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;

        private Guid? filterUser;
        private DateTime? createdFrom;
        private DateTime? createdTo;
        private DateTime? closedFrom;
        private DateTime? closedTo;

        private Guid? FilterUser
        {
            get => filterUser;
            set
            {
                filterUser = value;
                ApplyFilters();
            }
        }

        private DateTime? CreatedFrom
        {
            get => createdFrom;
            set
            {
                createdFrom = value;
                ApplyFilters();
            }
        }

        private DateTime? CreatedTo
        {
            get => createdTo;
            set
            {
                createdTo = value;
                ApplyFilters();
            }
        }

        private DateTime? ClosedFrom
        {
            get => closedFrom;
            set
            {
                closedFrom = value;
                ApplyFilters();
            }
        }

        private DateTime? ClosedTo
        {
            get => closedTo;
            set
            {
                closedTo = value;
                ApplyFilters();
            }
        }
        Guid DraggedTaskId;

        User CurrentUser = new();
        List<TaskItem> AllTasks = new();
        List<TaskItem> FilteredTasks = new();
        List<User> AllUsers = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                AllTasks = (await taskApiService.GetTasksAsync())?.ToList();
                AllUsers = (await userApiService.GetAllUsersAsync())?.ToList();
                CurrentUser = await userApiService.GetCurrentUserAsync();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("401"))
                    Navigation.NavigateTo("/login");
                Console.WriteLine(ex.Message);

            }
        }

        readonly Dictionary<StatusTask, string> statusLabels = new()
        {
            { StatusTask.Created, "Created" },
            { StatusTask.InProgress, "In Progress" },
            { StatusTask.Done, "Completed" }
        };
        private CreateTaskModal? ModalRef;

        private async Task AddTask(TaskItem newTask)
        {
            AllTasks.Add(await taskApiService.CreateTaskAsync(newTask));
            ApplyFilters();
            StateHasChanged();
        }

        private ViewTaskModal? ViewModal;

        private async Task OpenViewModal(TaskItem task)
        {
            ViewModal!.Task = task;
            ViewModal.Show();
        }

        private async Task SaveTask(TaskItem task)
        {
            await taskApiService.UpdateTaskAsync(task.Id, task);
        }
        private async Task DeleteTask(TaskItem task)
        {
            await taskApiService.DeleteTaskAsync(task.Id);
            AllTasks.Remove(task);
            ApplyFilters();
            StateHasChanged();
        }

        private void ApplyFilters(ChangeEventArgs? _ = null)
        {
            FilteredTasks = AllTasks
                .Where(t => !FilterUser.HasValue || t.CreatedByUserId == FilterUser)
                .Where(t => !CreatedFrom.HasValue || t.CreatedAt.Date >= CreatedFrom.Value.Date)
                .Where(t => !CreatedTo.HasValue || t.CreatedAt.Date <= CreatedTo.Value.Date)
                .Where(t => !ClosedFrom.HasValue || (t.ClosedAt.HasValue && t.ClosedAt.Value.Date >= ClosedFrom.Value.Date))
                .Where(t => !ClosedTo.HasValue || (t.ClosedAt.HasValue && t.ClosedAt.Value.Date <= ClosedTo.Value.Date))
                .ToList();
        }

        void OnDragStart(DragEventArgs e, Guid taskId)
        {
            DraggedTaskId = taskId;
        }

        async Task OnDrop(TaskItem task, StatusTask newStatus)
        {
            if (task is not null && task.Status != newStatus && task.CreatedByUserId == CurrentUser.Id)
            {
                task.Status = newStatus;
                await taskApiService.UpdateTaskAsync(task.Id, task);
            }

            StateHasChanged();
        }
    }
}

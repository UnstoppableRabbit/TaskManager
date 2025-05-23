using Domain.Enums;
using Domain.Model;
using Microsoft.AspNetCore.Components;

namespace TaskManager.Web.Components
{
    public partial class ViewTaskModal
    {
        [Parameter] public TaskItem Task { get; set; } = default!;
        [Parameter] public User CurrentUser { get; set; } = default!;
        [Parameter] public EventCallback<TaskItem> OnTaskUpdated { get; set; }
        [Parameter] public EventCallback<TaskItem> OnTaskDeleted { get; set; }

        private string? ActualDurationText;
        private string? NewComment;
        private bool IsVisible = false;
        public TaskItem taskCopy { get; set; } = default!;
        public void Show()
        {
            ActualDurationText = Task.ActualDuration?.ToString(@"hh\:mm");
            IsVisible = true;
            taskCopy = new TaskItem
            {
                Title = Task.Title,
                Description = Task.Description,
                Status = Task.Status,
                CreatedAt = Task.CreatedAt,
                ClosedAt = Task.ClosedAt,
                ActualDuration = Task.ActualDuration,
                Comments = Task.Comments.ToList(),
                CreatedByUserId = Task.CreatedByUserId
            };
            StateHasChanged();
        }

        private async Task Delete()
        {
            IsVisible = false;
            await OnTaskDeleted.InvokeAsync(Task);
        }

        private void Close() => IsVisible = false;

        private void ParseDuration()
        {
            if (TimeSpan.TryParse(ActualDurationText, out var parsed))
                taskCopy.ActualDuration = parsed;
        }

        private void AddComment()
        {
            if (!string.IsNullOrWhiteSpace(NewComment))
            {
                taskCopy.Comments.Add(new Comment
                {
                    CreatedAt = DateTime.UtcNow,
                    Text = NewComment.Trim(),
                    UserId = Guid.Empty
                });
                NewComment = string.Empty;
            }
        }

        private async Task Submit()
        {
            Task.Status = taskCopy.Status;
            Task.Comments = taskCopy.Comments;
            Task.ActualDuration = taskCopy.ActualDuration;
            Task.ClosedAt = taskCopy.ClosedAt;

            if (Task.Status == StatusTask.Done && !Task.ClosedAt.HasValue)
                Task.ClosedAt = DateTime.UtcNow;

            await OnTaskUpdated.InvokeAsync(Task);
            IsVisible = false;
        }
    }
}

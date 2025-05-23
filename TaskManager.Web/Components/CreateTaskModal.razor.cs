using Domain.Enums;
using Domain.Model;
using Microsoft.AspNetCore.Components;

namespace TaskManager.Web.Components
{
    public partial class CreateTaskModal
    {
        [Parameter] public EventCallback<TaskItem> OnTaskCreated { get; set; }

        private TaskItem NewTask = new();
        private string? NewComment;
        private bool IsVisible = false;
        private string? ActualDurationText;

        public void Show()
        {
            NewTask = new TaskItem
            {
                CreatedAt = DateTime.UtcNow,
                Status = StatusTask.Created
            };
            IsVisible = true;
            StateHasChanged();
        }

        private void Close() => IsVisible = false;

        private void AddComment()
        {
            if (!string.IsNullOrWhiteSpace(NewComment))
            {
                NewTask.Comments.Add(new Comment { CreatedAt = DateTime.Now, Text = NewComment.Trim(), UserId = Guid.Empty });
                NewComment = string.Empty;
            }
        }

        private void ParseDuration()
        {
            if (TimeSpan.TryParse(ActualDurationText, out var parsed))
            {
                NewTask.ActualDuration = parsed;
            }
            else
            {
                NewTask.ActualDuration = null;
            }
        }

        void ParseDuration(string? input)
        {
            if (TimeSpan.TryParse(input, out var span))
                NewTask.ActualDuration = span;
        }

        private async Task Submit()
        {
            if (NewTask.Status == StatusTask.Done && !NewTask.ClosedAt.HasValue)
                NewTask.ClosedAt = DateTime.UtcNow;

            await OnTaskCreated.InvokeAsync(NewTask);
            IsVisible = false;
        }
    }
}

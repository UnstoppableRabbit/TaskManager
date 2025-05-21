using Domain.Enums;

namespace Domain.Model
{
    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public DateTime? EstimatedFinishAt { get; set; }
        public TimeSpan? ActualDuration { get; set; }
        public StatusTask Status { get; set; } = StatusTask.Created;
        public Guid CreatedByUserId { get; set; }
        public List<Comment> Comments { get; set; } = new();
    }
}

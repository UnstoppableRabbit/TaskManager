﻿namespace Domain.Model
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<TaskItem> Tasks { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
    }
}

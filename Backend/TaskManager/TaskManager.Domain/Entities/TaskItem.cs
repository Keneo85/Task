using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        private TaskItem() { }

        public void SetDetails(Guid categotyId, string title, string? description)
        {
            if (categotyId == Guid.Empty)
            {
                throw new DomainException("La categoria es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3 || title.Trim().Length > 100)
            {
                throw new DomainException("El titulo debe de tener 3 y 100 caracteres.");
            }

            if (description is not null && description.Length > 500)
            {
                throw new DomainException("La descripcion no puede superar los 500 caracteres.");
            }

            CategoryId = categotyId;
            Title = title;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        public TaskItem(Guid userId, Guid categoryId, string title, string? description)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            IsCompleted = false;
            CreatedAt = DateTime.UtcNow;
            SetDetails(categoryId, title, description);
        }

        public void Update(Guid categotyId, string title, string? description, bool isCompleted)
        {
            SetDetails(categotyId, title, description);
            IsCompleted = isCompleted;
        }

        public bool BelongsTo(Guid userId) => UserId == userId;
    }
}

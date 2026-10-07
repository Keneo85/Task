using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        private User() { }

        public User(string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DomainException("El Email es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new DomainException("La contraseña es obligatoria");
            }

            Id = Guid.NewGuid();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }
    }
}

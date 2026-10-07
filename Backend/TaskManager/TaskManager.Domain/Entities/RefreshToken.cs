using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        private RefreshToken() { }

        public RefreshToken(Guid userId, string tokeHash, DateTime expiresAt)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            TokenHash = tokeHash;
            ExpiresAt = expiresAt;
        }

        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

        public void Revoke() => RevokedAt ??= DateTime.UtcNow;
    }
}

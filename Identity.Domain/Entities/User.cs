using Identity.Domain.Primitives;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    public class User : IdentityUser<Guid>, IEntity<Guid>
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public DateTime? OtpRequestedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int OtpRequestCountToday { get; set; }
        public DateTime? OtpRequestCountResetAt { get; set; }
        public bool EnableNotification { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = [];

   

    }

}

using Identity.Shared.Identity;
using Identity.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastrcture.Intersptors
{
    public class AuditInterceptor(ICurrentUserService currentUserService, ILogger<AuditInterceptor> logger) : SaveChangesInterceptor

    {
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<AuditInterceptor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await SetAuditPropertiesAsync(eventData.Context, cancellationToken);
            }
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                SetAuditProperties(eventData.Context);
            }
            return base.SavingChanges(eventData, result);
        }

        private async Task SetAuditPropertiesAsync(DbContext context, CancellationToken cancellationToken)
        {
            SetAuditProperties(context);
            await Task.CompletedTask;
        }

        private void SetAuditProperties(DbContext context)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId() ?? Guid.Empty;
                var currentTime = DateTime.UtcNow;

                var entries = context.ChangeTracker.Entries()
                    .Where(e => IsAuditableEntity(e.Entity))
                    .ToList();

                foreach (var entry in entries)
                {
                    ProcessEntityEntry(entry, currentUserId, currentTime);
                }

                _logger.LogDebug("Processed {EntryCount} auditable entities", entries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while setting audit properties");
                throw;
            }
        }

        private static bool IsAuditableEntity(object entity)
        {
            var entityType = entity.GetType();

            // Check if entity inherits from BaseEntity<T>
            var currentType = entityType;
            while (currentType != null)
            {
                if (currentType.IsGenericType &&
                    currentType.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }

            return false;
        }

        private void ProcessEntityEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Guid currentUserId, DateTime currentTime)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    SetCreationAuditProperties(entry, currentUserId, currentTime);
                    break;

                case EntityState.Modified:
                    SetModificationAuditProperties(entry, currentUserId, currentTime);
                    break;

                case EntityState.Deleted:
                    SetSoftDeleteProperties(entry, currentUserId, currentTime);
                    break;
            }
        }

        private static void SetCreationAuditProperties(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Guid currentUserId, DateTime currentTime)
        {
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.CreatedBy = currentUserId;
                auditableEntity.CreatedDate = currentTime;
                auditableEntity.IsDeleted = false;
            }
        }

        private static void SetModificationAuditProperties(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Guid currentUserId, DateTime currentTime)
        {
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                auditableEntity.ModifiedBy = currentUserId;
                auditableEntity.ModifiedDate = currentTime;

                entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.CreatedDate)).IsModified = false;
            }
        }

        private void SetSoftDeleteProperties(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, Guid currentUserId, DateTime currentTime)
        {
            if (entry.Entity is ISoftDeletable softDeletableEntity)
            {
                entry.State = EntityState.Modified;

                softDeletableEntity.IsDeleted = true;
                softDeletableEntity.DeletedBy = currentUserId;
                softDeletableEntity.DeletedDate = currentTime;

                _logger.LogDebug("Converted hard delete to soft delete for entity {EntityType} with ID {EntityId}",
                    entry.Entity.GetType().Name,
                    entry.Property("Id")?.CurrentValue);
            }
        }
    }
}

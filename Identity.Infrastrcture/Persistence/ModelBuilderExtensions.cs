using System.Linq.Expressions;
using Identity.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastrcture.Persistence
{
    public static class ModelBuilderExtensions
    {
        public static void ApplySoftDeleteQueryFilter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                    var compareExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                    var lambda = Expression.Lambda(compareExpression, parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }
}
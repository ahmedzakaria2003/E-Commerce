namespace Identity.Domain.Primitives;

public abstract class BaseEntity<TKey> : IEntity<TKey>, IAuditableEntity, ISoftDeletable
{
    public TKey Id { get; set; } = default!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
}

public interface IEntity<TId>
{
    TId Id { get; set; }
}

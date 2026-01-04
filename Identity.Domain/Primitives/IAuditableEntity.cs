namespace Identity.Domain.Primitives;

public interface IAuditableEntity
{
    Guid CreatedBy { get; set; }
    DateTime CreatedDate { get; set; }
    Guid? ModifiedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
    bool IsDeleted { get; set; }
}

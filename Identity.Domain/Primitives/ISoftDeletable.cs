namespace Identity.Domain.Primitives;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    Guid? DeletedBy { get; set; }
    DateTime? DeletedDate { get; set; }
}

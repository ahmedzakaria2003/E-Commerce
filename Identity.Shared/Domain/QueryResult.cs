namespace Identity.Shared.Domain;

public class QueryResult<TEntity>
{
    public int TotalItems { get; set; }
    public IEnumerable<TEntity> Items { get; set; } = [];
}

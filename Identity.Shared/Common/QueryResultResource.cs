namespace Identity.Shared.Common;

public class QueryResultResource<TResource>
{
    public int TotalItems { get; set; }
    public IEnumerable<TResource> Items { get; set; } = [];
}

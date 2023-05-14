namespace RestDWH.Model
{
    public class FieldsListBase
    {
        public long From { get; set; }
        public long Size { get; set; }
        public long TotalCount { get; set; }
        public IEnumerable<Dictionary<string, object>> Results { get; set; } = Enumerable.Empty<Dictionary<string, object>>();
    }
}

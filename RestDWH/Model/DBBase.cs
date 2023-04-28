namespace RestDWH.Model
{
    public class DBBase<TComm>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public TComm? Data { get; set; } = default;
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public DateTimeOffset? Deleted { get; set; } = default;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? DeletedBy { get; set; }
    }
}

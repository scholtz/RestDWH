namespace RestDWH.Base.Model
{
    public class DBListBase<TComm, T> where T : DBBase<TComm>
    {
        public long Offset { get; set; }
        public long Limit { get; set; }
        public long TotalCount { get; set; }
        public T[] Results { get; set; } = Array.Empty<T>();
    }
}

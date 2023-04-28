namespace RestDWH.Model
{
    public class DBListBase<TComm, T> where T : DBBase<TComm>
    {
        public long From { get; set; }
        public long Size { get; set; }
        public long TotalCount { get; set; }
        public T[] Results { get; set; } = Array.Empty<T>();
    }
}

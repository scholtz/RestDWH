namespace RestDWH.Model
{
    public class DBBaseLog<T> : DBBase<T>
    {
        public string RefId { get; set; }
        public long Version { get; set; }
    }
}

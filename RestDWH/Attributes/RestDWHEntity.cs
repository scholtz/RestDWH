namespace RestDWH.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RestDWHEntity : System.Attribute
    {
        public string Name { get; set; }
        public Type? Events { get; set; }

        public RestDWHEntity(string name, Type? events = null)
        {
            Name = name;
            Events = events;
        }
    }
}

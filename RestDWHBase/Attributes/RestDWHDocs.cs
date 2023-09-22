namespace RestDWH.Base.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct)
]
    public class RestDWHDocsAttribute : System.Attribute
    {
        public string Name;
        public string Summary;
        public string Description;
        public double Version;

        public RestDWHDocsAttribute(string name, string summary, string description)
        {
            Name = name;
            Version = 1.0;
            Summary = summary;
            Description = description;
        }
    }
}

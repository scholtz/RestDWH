namespace RestDWH.Base.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class |
                       System.AttributeTargets.Struct)
]
    public class RestDWHEntity : System.Attribute
    {
        public string Name { get; set; }
        public Type? Events { get; set; }
        public string? EndpointGet { get; private set; }
        public string? EndpointGetWithFields { get; private set; }
        public string? EndpointGetById { get; private set; }
        public string? EndpointGetByIdWithFields { get; private set; }
        public string? EndpointPost { get; private set; }
        public string? EndpointPut { get; private set; }
        public string? EndpointUpsert { get; private set; }
        public string? EndpointPatch { get; private set; }
        public string? EndpointDelete { get; private set; }
        public string? EndpointProperties { get; private set; }
        public string? ApiName { get; private set; }

        public RestDWHEntity(
            string name,
            Type? events = null,
            string? apiName = null,
            string? endpointGet = null,
            string? endpointGetWithFields = null,
            string? endpointGetById = null,
            string? endpointGetByIdWithFields = null,
            string? endpointPost = null,
            string? endpointPut = null,
            string? endpointUpsert = null,
            string? endpointPatch = null,
            string? endpointDelete = null,
            string? endpointProperties = null
        )
        {
            Name = name;
            ApiName = apiName;
            Events = events;
            EndpointGet = endpointGet;
            EndpointGetWithFields = endpointGetWithFields;
            EndpointGetById = endpointGetById;
            EndpointGetByIdWithFields = endpointGetByIdWithFields;
            EndpointPost = endpointPost;
            EndpointPut = endpointPut;
            EndpointUpsert = endpointUpsert;
            EndpointPatch = endpointPatch;
            EndpointDelete = endpointDelete;
            EndpointProperties = endpointProperties;
        }
        public string MainTable => $"restdwh-{Name.ToLowerInvariant()}-main";
        public string LogTable => $"restdwh-{Name.ToLowerInvariant()}-main";
        public string ApiNameOrName => ApiName ?? Name.ToLowerInvariant();

    }
}

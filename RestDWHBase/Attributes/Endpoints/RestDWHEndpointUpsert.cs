namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointUpsert : Attribute
    {
        public string Path;

        public RestDWHEndpointUpsert(string path = "v1/{ApiName}")
        {
            Path = path;
        }
    }
}

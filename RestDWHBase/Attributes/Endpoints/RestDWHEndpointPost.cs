namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointPost : Attribute
    {
        public string Path;

        public RestDWHEndpointPost(string path = "v1/{ApiName}")
        {
            Path = path;
        }
    }
}

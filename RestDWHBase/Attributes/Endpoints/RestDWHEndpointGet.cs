namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointGet : Attribute
    {
        public string Path;

        public RestDWHEndpointGet(string path = "v1/{ApiName}")
        {
            Path = path;
        }
    }
}

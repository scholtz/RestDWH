namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointDelete : Attribute
    {
        public string Path;

        public RestDWHEndpointDelete(string path = "v1/{ApiName}")
        {
            Path = path;
        }
    }
}

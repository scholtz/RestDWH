namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointPut : Attribute
    {
        public string Path;

        public RestDWHEndpointPut(string path = "v1/{ApiName}")
        {
            Path = path;
        }
    }
}

namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointPatch : Attribute
    {
        public string Path;

        public RestDWHEndpointPatch(string path = "v1/{ApiName}")
        {
            Path = path;
        }
    }
}

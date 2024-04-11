namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointGetById : Attribute
    {
        public string Path;

        public RestDWHEndpointGetById(string path = "v1/{ApiName}/{id}")
        {
            Path = path;
        }
    }
}

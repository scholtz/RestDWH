namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointProperties : Attribute
    {
        public string Path;

        public RestDWHEndpointProperties(string path = "v1/{ApiName}/properties/{propertyId}")
        {
            Path = path;
        }
    }
}

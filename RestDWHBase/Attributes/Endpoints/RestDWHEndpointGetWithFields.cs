namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointGetWithFields : Attribute
    {
        public string Path;

        public RestDWHEndpointGetWithFields(string path = "v1/{ApiName}/custom-fields")
        {
            Path = path;
        }
    }
}

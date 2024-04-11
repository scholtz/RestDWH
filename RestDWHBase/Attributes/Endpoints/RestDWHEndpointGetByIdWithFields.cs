namespace RestDWHBase.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointGetByIdWithFields : Attribute
    {
        public string Path;

        public RestDWHEndpointGetByIdWithFields(string path = "v1/{ApiName}/{id}/custom-fields")
        {
            Path = path;
        }
    }
}

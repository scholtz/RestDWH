namespace RestDWH.Elastic.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointElasticPropertiesQuery : Attribute
    {
        public string Path;

        public RestDWHEndpointElasticPropertiesQuery(string path = "v1/{ApiName}/properties/query")
        {
            Path = path;
        }
    }
}

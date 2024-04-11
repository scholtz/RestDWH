namespace RestDWH.Elastic.Attributes.Endpoints
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class RestDWHEndpointElasticQuery : Attribute
    {
        public string Path;

        public RestDWHEndpointElasticQuery(string path = "v1/{ApiName}/query")
        {
            Path = path;
        }
    }
}

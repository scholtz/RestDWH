using Nest;
using RestDWH.Base.Attributes;
using RestDWH.Base.Model;
using System.Reflection;

namespace RestDWH.Elastic.Extensions
{
    public static class ElasticSearchExtensions
    {
        public static ConnectionSettings ExtendElasticConnectionSettings(this ConnectionSettings conn)
        {
            IEnumerable<Type> enumerable = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                           from type in assembly.GetTypes()
                                           where type.IsDefined(typeof(RestDWHEntity))
                                           select type;
            ConnectionSettings connectionSettings = conn;
            foreach (Type item in enumerable)
            {
                RestDWHEntity[] source = (RestDWHEntity[])Attribute.GetCustomAttributes(item, typeof(RestDWHEntity));
                string name = (source.FirstOrDefault()?.Name ?? "obj").ToLower();
                Type type2 = typeof(DBBase<>)!.MakeGenericType(item);
                typeof(DBListBase<,>)!.MakeGenericType(item, type2);
                Type documentType = typeof(DBBaseLog<>)!.MakeGenericType(item);
                connectionSettings = connectionSettings.DefaultMappingFor(type2, (ClrTypeMappingDescriptor r) => r.IndexName("restdwh-" + name + "-main"));
                connectionSettings = connectionSettings.DefaultMappingFor(documentType, (ClrTypeMappingDescriptor r) => r.IndexName("restdwh-" + name + "-log"));
            }

            return connectionSettings;
        }
    }
}

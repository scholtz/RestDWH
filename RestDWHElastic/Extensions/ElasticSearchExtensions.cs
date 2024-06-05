using Nest;
using RestDWH.Base.Attributes;
using RestDWH.Base.Model;
using System.Reflection;

namespace RestDWH.Elastic.Extensions
{
    public static class ElasticSearchExtensions
    {
        public static ConnectionSettings ExtendElasticConnectionSettings(this ConnectionSettings conn, RestDWH.Elastic.Model.Config.Elastic config)
        {
            IEnumerable<Type> enumerable = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                           from type in assembly.GetTypes()
                                           where type.IsDefined(typeof(RestDWHEntity))
                                           select type;
            ConnectionSettings connectionSettings = conn;
            foreach (Type item in enumerable)
            {
                RestDWHEntity[] source = (RestDWHEntity[])Attribute.GetCustomAttributes(item, typeof(RestDWHEntity));
                var entity = source.FirstOrDefault();
                var name = (entity?.Name ?? "obj").ToLower();

                Type type2 = typeof(DBBase<>)!.MakeGenericType(item);
                typeof(DBListBase<,>)!.MakeGenericType(item, type2);
                connectionSettings = connectionSettings.DefaultMappingFor(type2, (ClrTypeMappingDescriptor r) => r.IndexName($"{config.IndexPrefix}{name}{config.IndexSuffixMain}"));

                // LOG
                Type documentType = typeof(DBBaseLog<>)!.MakeGenericType(item);
                connectionSettings = connectionSettings.DefaultMappingFor(documentType, (ClrTypeMappingDescriptor r) => r.IndexName($"{config.IndexPrefix}{name}{config.IndexSuffixLog}"));
            }

            return connectionSettings;
        }
    }
}

using RestDWH.Elastic.Model;
using Microsoft.Extensions.Options;
using RestDWH.Elastic.Migration;

namespace RestDWH.Elastic.Repository
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEnt"></typeparam>
    public class ElasticContextBase<TEnt>
    {
        private IElasticMigrationBuilder<TEnt> _elasticMigrationBuilder;
        private IOptionsMonitor<RestDWH.Elastic.Model.Config.Elastic> _config;
        private readonly string MigrationIndexName;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticMigrationBuilder"></param>
        public ElasticContextBase(
            IElasticMigrationBuilder<TEnt> elasticMigrationBuilder,
            IOptionsMonitor<RestDWH.Elastic.Model.Config.Elastic> config
            )
        {
            MigrationIndexName = $"{config.CurrentValue.IndexPrefix}{typeof(TEnt).Name.ToLower()}{config.CurrentValue.IndexSuffixMain}-migration";
            _elasticMigrationBuilder = elasticMigrationBuilder;
            _config = config;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void EnsureCreated()
        {
            try
            {
                var response = _elasticMigrationBuilder.IndexExistsAsync(MigrationIndexName).Result;
                if (!response.Exists)
                {
                    _elasticMigrationBuilder.CreateIndexMigrationAsync(MigrationIndexName).Wait();
                }
                if (!_config.CurrentValue.ApplyMigration) return;

                var migrations = _elasticMigrationBuilder.GetAllMigrationsAsync(MigrationIndexName).Result;

                string mynamespace = $"{this.GetType().Assembly.GetName().Name}.Migrations";

                var types = (from t in this.GetType().Assembly.GetTypes()
                             where t.IsClass && t.Namespace == mynamespace && t.BaseType == typeof(ElasticMigration)
                             select t).OrderBy(t => t.Name);

                foreach (var t in types)
                {
                    if (t.IsClass && !migrations.Any(m => m.Name == t.Name))
                    {
                        var instance = Activator.CreateInstance(t) as ElasticMigration;
                        instance?.Up(_elasticMigrationBuilder, _config.CurrentValue);
                        _elasticMigrationBuilder.PostMigrationAsync(MigrationIndexName, new RestDWH.Elastic.Model.ElasticMigrationEnt()
                        {
                            Name = t.Name
                        }).Wait();
                    }
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}

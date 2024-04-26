using RestDWH.Elastic.Model;

namespace RestDWH.Elastic.Migration
{
    public abstract class ElasticMigration
    {
        public abstract void Up<TEnt>(IElasticMigrationBuilder<TEnt> migrationBuilder, RestDWH.Elastic.Model.Config.Elastic config);
    }
}

namespace RestDWH.Elastic.Model
{
    public class ElasticMigrationEnt
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }
    }
}

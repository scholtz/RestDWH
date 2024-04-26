using Nest;

namespace RestDWH.Elastic.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEnt"></typeparam>
    public interface IElasticMigrationBuilder<TEnt>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task<CreateIndexResponse> CreateIndexAsync(string indexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task<CreateIndexResponse> CreateIndexMigrationAsync(string indexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task<UpdateIndexSettingsResponse> UpdateSettingsAsync(string indexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceIndexName"></param>
        /// <param name="destinationIndexName"></param>
        /// <returns></returns>
        Task<ReindexOnServerResponse> ReIndexAsync(string sourceIndexName, string destinationIndexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task SetTextAnalyzerAsync(string indexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task<ExistsResponse> IndexExistsAsync(string indexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        Task<List<ElasticMigrationEnt>> GetAllMigrationsAsync(string indexName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="migration"></param>
        /// <returns></returns>
        Task PostMigrationAsync(string indexName, ElasticMigrationEnt migration);
    }
}

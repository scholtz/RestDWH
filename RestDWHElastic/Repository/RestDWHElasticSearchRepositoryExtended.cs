using Nest;
using RestDWH.Base.Attributes;
using RestDWH.Base.Model;
using RestDWHElastic.Repository;
using System.Reflection;

namespace RestDWH.Elastic.Repository
{
    public class RestDWHElasticSearchRepositoryExtended<TEnt> : IElasticDWHRepository<TEnt>
        where TEnt : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly RestDWHEvents<TEnt> _events;
        private readonly ILogger<RestDWHElasticSearchRepository<TEnt>> _logger;
        public RestDWHElasticSearchRepositoryExtended(IElasticClient elasticClient, RestDWHEvents<TEnt> events, ILogger<RestDWHElasticSearchRepository<TEnt>> logger)
        {
            _elasticClient = elasticClient;
            _events = events;
            _logger = logger;
        }

        public virtual async Task<DBListBase<TEnt, DBBase<TEnt>>> GetAsync(string query, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var config = typeof(TEnt).GetCustomAttribute<RestDWHEntity>();
            if (config == null) { throw new Exception($"Config not found for {typeof(TEnt)}"); }
            //(offset, limit, query, sort) = await _events.BeforeGetAsync(offset, limit, query, sort, user);
            var searchParams = new Elasticsearch.Net.SearchRequestParameters();
            var ret = await _elasticClient.LowLevel.SearchAsync<SearchResponse<DBBase<TEnt>>>(config.MainTable, query, searchParams);
            if (!string.IsNullOrEmpty(ret.OriginalException?.Message)) throw new Exception(ret.OriginalException?.Message);
            var list = ret.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();
            var instance = Activator.CreateInstance(typeof(DBListBase<TEnt, DBBase<TEnt>>)) as DBListBase<TEnt, DBBase<TEnt>>;
            if (instance == null) throw new Exception("Unable to inicialize DBListBase<TEnt, DBBase<TEnt>>");
            instance.Results = list;
            instance.Offset = 0;
            instance.Limit = 0;
            instance.TotalCount = ret.Total;
            //var result = await _events.AfterGetAsync(instance, offset, limit, query, sort, user);
            return instance;
        }
        /// <summary>
        /// Get properties from
        /// </summary>
        /// <param name="query"></param>
        /// <param name="attribute"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<List<object>?> GetPropertiesAsync(string query, string attribute, int offset = 0, int limit = 10, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var config = typeof(TEnt).GetCustomAttribute<RestDWHEntity>();
            if (config == null) { throw new Exception($"Config not found for {typeof(TEnt)}"); }
            //(offset, limit, query, sort) = await _events.BeforeGetAsync(offset, limit, query, sort, user);
            var searchParams = new Elasticsearch.Net.SearchRequestParameters();
            var ret = await _elasticClient.LowLevel.SearchAsync<SearchResponse<DBBase<TEnt>>>(config.MainTable, query, searchParams);

            if (!string.IsNullOrEmpty(ret.OriginalException?.Message)) throw new Exception(ret.OriginalException?.Message);
            if (!ret.Aggregations.Any()) throw new Exception("No aggregations");
            var result = ret.Aggregations.Terms(attribute).Buckets.Where(s => (s.Key != "0" || (s.Key == "0" && !string.IsNullOrEmpty(s.KeyAsString))) && !string.IsNullOrEmpty(s.Key)).Select(s => s.Key as object).Skip(offset).Take(limit).ToList();
            return result;
        }
    }
}

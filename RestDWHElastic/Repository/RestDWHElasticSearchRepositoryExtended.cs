using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using RestDWH.Base.Attributes;
using RestDWH.Elastic.Model;
using System.Reflection;

namespace RestDWH.Elastic.Repository
{
    public class RestDWHElasticSearchRepositoryExtended<TEnt> : IElasticDWHRepository<TEnt>
        where TEnt : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly RestDWHEventsElastic<TEnt> _events;
        private readonly ILogger<RestDWHElasticSearchRepository<TEnt>> _logger;
        private readonly IOptionsMonitor<Model.Config.Elastic> _config;
        private readonly IServiceProvider _serviceProvider;
        public RestDWHElasticSearchRepositoryExtended(
            IElasticClient elasticClient,
            RestDWHEventsElastic<TEnt> events,
            ILogger<RestDWHElasticSearchRepository<TEnt>> logger,
            IOptionsMonitor<Model.Config.Elastic> config,
            IServiceProvider serviceProvider
            )
        {
            _elasticClient = elasticClient;
            _events = events;
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Returns the elastic index for log data
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetLogIndex()
        {
            var config = typeof(TEnt).GetCustomAttribute<RestDWHEntity>();
            if (config == null) { throw new Exception($"Config not found for {typeof(TEnt)}"); }
            return $"{_config.CurrentValue.IndexPrefix}{config.MainTable}{_config.CurrentValue.IndexSuffixLog}";
        }
        /// <summary>
        /// Returns the elastic index for main data
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetMainIndex()
        {
            var config = typeof(TEnt).GetCustomAttribute<RestDWHEntity>();
            if (config == null) { throw new Exception($"Config not found for {typeof(TEnt)}"); }
            return $"{_config.CurrentValue.IndexPrefix}{config.MainTable}{_config.CurrentValue.IndexSuffixMain}";
        }

        public virtual async Task<Base.Model.DBListBase<TEnt, Base.Model.DBBase<TEnt>>> QueryAsync(string query, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            await _events.BeforeEachAsync(user, _serviceProvider);
            var config = typeof(TEnt).GetCustomAttribute<RestDWHEntity>();
            if (config == null) { throw new Exception($"Config not found for {typeof(TEnt)}"); }
            (query) = await _events.BeforeQueryAsync(query, user, _serviceProvider);
            var searchParams = new Elasticsearch.Net.SearchRequestParameters();
            var ret = await _elasticClient.LowLevel.SearchAsync<SearchResponse<Base.Model.DBBase<TEnt>>>(GetMainIndex(), query, searchParams);
            if (!string.IsNullOrEmpty(ret.OriginalException?.Message)) throw new Exception(ret.OriginalException?.Message);
            var list = ret.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();
            var instance = Activator.CreateInstance(typeof(Base.Model.DBListBase<TEnt, Base.Model.DBBase<TEnt>>)) as Base.Model.DBListBase<TEnt, Base.Model.DBBase<TEnt>>;
            if (instance == null) throw new Exception("Unable to inicialize DBListBase<TEnt, DBBase<TEnt>>");
            instance.Results = list;
            instance.Offset = 0;
            instance.Limit = 0;
            instance.TotalCount = ret.Total;
            var result = await _events.AfterQueryAsync(instance, query, user, _serviceProvider);
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
            await _events.BeforeEachAsync(user, _serviceProvider);
            var config = typeof(TEnt).GetCustomAttribute<RestDWHEntity>();
            if (config == null) { throw new Exception($"Config not found for {typeof(TEnt)}"); }
            (query, attribute, offset, limit) = await _events.BeforeGetPropertiesAsync(query, attribute, offset, limit, user, _serviceProvider);
            var searchParams = new Elasticsearch.Net.SearchRequestParameters();
            var ret = await _elasticClient.LowLevel.SearchAsync<SearchResponse<Base.Model.DBBase<TEnt>>>(GetMainIndex(), query, searchParams);

            if (!string.IsNullOrEmpty(ret.OriginalException?.Message)) throw new Exception(ret.OriginalException?.Message);
            if (!ret.Aggregations.Any()) throw new Exception("No aggregations");
            var result = ret.Aggregations.Terms(attribute).Buckets.Where(s => (s.Key != "0" || (s.Key == "0" && !string.IsNullOrEmpty(s.KeyAsString))) && !string.IsNullOrEmpty(s.Key)).Select(s => s.Key as object).Skip(offset).Take(limit).ToList();
            return result;
        }
    }
}

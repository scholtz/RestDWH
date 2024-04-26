using RestDWH.Elastic.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using RestDWH.Base.Model;

namespace RestDWH.Elastic.Migration
{
    public class ElasticMigrationBuilder<TEnt> : IElasticMigrationBuilder<TEnt>
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticMigrationBuilder<TEnt>> _logger;
        private readonly IOptionsMonitor<RestDWH.Elastic.Model.Config.Elastic> _config;
        public ElasticMigrationBuilder(
            IElasticClient elasticClient,
            ILogger<ElasticMigrationBuilder<TEnt>> logger,
            IOptionsMonitor<RestDWH.Elastic.Model.Config.Elastic> config)
        {
            _elasticClient = elasticClient;
            _logger = logger;
            _config = config;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public virtual async Task<CreateIndexResponse> CreateIndexAsync(string indexName)
        {
            //create index  
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(Indices.Index(indexName), i =>
                                                                 i.Settings(settings => settings
                                                                    .Analysis(analysis => analysis
                                                                        .Analyzers(analyzers => analyzers
                                                                               .Custom("text_analyzer", cust => cust
                                                                                        .Tokenizer("standard")
                                                                                        .Filters("lowercase", "asciifolding")))))
                                                                 .Map<DBBase<TEnt>>(m => m.AutoMap()));

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError(createIndexResponse.OriginalException, createIndexResponse.DebugInformation);
                throw createIndexResponse.OriginalException;
            }
            return createIndexResponse;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public virtual async Task<CreateIndexResponse> CreateIndexMigrationAsync(string indexName)
        {
            //create index  
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(Indices.Index(indexName), i =>
                                                                 i.Settings(settings => settings
                                                                    .Analysis(analysis => analysis
                                                                        .Analyzers(analyzers => analyzers
                                                                               .Custom("text_analyzer", cust => cust
                                                                                        .Tokenizer("standard")
                                                                                        .Filters("lowercase", "asciifolding")))))
                                                                 .Map<ElasticMigrationEnt>(m => m.AutoMap()));

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError(createIndexResponse.OriginalException, createIndexResponse.DebugInformation);
                throw createIndexResponse.OriginalException;
            }
            return createIndexResponse;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public virtual async Task<UpdateIndexSettingsResponse> UpdateSettingsAsync(string indexName)
        {
            //close index
            var closeResponseIndex = await _elasticClient.Indices.CloseAsync(Indices.Index(indexName));

            if (!closeResponseIndex.IsValid)
            {
                _logger.LogError(closeResponseIndex.OriginalException, closeResponseIndex.DebugInformation);
                throw closeResponseIndex.OriginalException;
            }

            var customAnalyzer = new CustomAnalyzer
            {
                Tokenizer = "standard",
                Filter = new[] { "lowercase", "asciifolding" }
            };
            //update settings
            var updateSettingsRequest = new UpdateIndexSettingsRequest(indexName)
            {
                IndexSettings = new IndexSettings
                {
                    Analysis = new Analysis
                    {
                        Analyzers = new Analyzers { { "text_analyzer", customAnalyzer } }
                    }
                },
            };

            var updateSettingsResponse = await _elasticClient.Indices.UpdateSettingsAsync(updateSettingsRequest);

            if (!updateSettingsResponse.IsValid)
            {
                _logger.LogError(updateSettingsResponse.OriginalException, updateSettingsResponse.DebugInformation);
                throw updateSettingsResponse.OriginalException;
            }

            //open index
            var openResponse = _elasticClient.Indices.Open(Indices.Index(indexName));
            if (!openResponse.IsValid)
            {
                _logger.LogError(openResponse.OriginalException, openResponse.DebugInformation);
                throw openResponse.OriginalException;
            }

            return updateSettingsResponse;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceIndexName"></param>
        /// <param name="destinationIndexName"></param>
        /// <returns></returns>
        public virtual async Task<ReindexOnServerResponse> ReIndexAsync(string sourceIndexName, string destinationIndexName)
        {
            //reindex
            var reindexResponse = await _elasticClient.ReindexOnServerAsync(r => r
            .Source(sou => sou.Index(sourceIndexName))
            .Destination(des => des.Index(destinationIndexName))
            .WaitForCompletion(true)
            );

            if (!reindexResponse.IsValid)
            {
                _logger.LogError(reindexResponse.OriginalException, reindexResponse.DebugInformation);
                throw reindexResponse.OriginalException;
            }
            return reindexResponse;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public virtual async Task<ExistsResponse> IndexExistsAsync(string indexName)
        {
            //exists index  
            var existsIndexResponse = await _elasticClient.Indices.ExistsAsync(Indices.Index(indexName));

            if (!existsIndexResponse.IsValid)
            {
                _logger.LogError(existsIndexResponse.OriginalException, existsIndexResponse.DebugInformation);
                //throw existsIndexResponse.OriginalException;
            }
            return existsIndexResponse;
        }

        public virtual async Task PostMigrationAsync(string indexName, ElasticMigrationEnt migration)
        {
            var node = _elasticClient.ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault();

            var settings =
            new ConnectionSettings(node?.Uri)
            .ApiKeyAuthentication(new Elasticsearch.Net.ApiKeyAuthenticationCredentials(_elasticClient.ConnectionSettings.ApiKeyAuthenticationCredentials.Base64EncodedApiKey))
            .EnableApiVersioningHeader();
            settings.DefaultIndex(indexName);
            var client = new ElasticClient(settings);

            var indexResponse = await client.IndexDocumentAsync(migration);
            if (!indexResponse.IsValid)
            {
                _logger.LogError(indexResponse.OriginalException, indexResponse.DebugInformation);
                throw indexResponse.OriginalException;
            }
        }

        public virtual async Task<List<ElasticMigrationEnt>> GetAllMigrationsAsync(string indexName)
        {
            var node = _elasticClient.ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault();

            var settings =
            new ConnectionSettings(node?.Uri)
            .ApiKeyAuthentication(new Elasticsearch.Net.ApiKeyAuthenticationCredentials(_elasticClient.ConnectionSettings.ApiKeyAuthenticationCredentials.Base64EncodedApiKey))
            .EnableApiVersioningHeader();
            settings.DefaultIndex(indexName);
            var client = new ElasticClient(settings);

            var searchResponse = await client.SearchAsync<ElasticMigrationEnt>(s =>
            {
                s = s.From(0);
                s = s.Size(10000);
                s = s.MatchAll();
                return s;
            });

            if (!searchResponse.IsValid)
            {
                _logger.LogError(searchResponse.OriginalException, searchResponse.DebugInformation);
                throw searchResponse.OriginalException;
            }

            var list = searchResponse.Hits.Select(s => { return s.Source; }).ToList();

            return list ?? new List<ElasticMigrationEnt>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public virtual async Task SetTextAnalyzerAsync(string indexName)
        {
            try
            {
                string name = typeof(TEnt).Name.ToLower();
                //create temp client
                string tempIndexName = $"{_config.CurrentValue.IndexPrefix}{name}{_config.CurrentValue.IndexSuffixMain}-temp-{Guid.NewGuid()}";

                var node = _elasticClient.ConnectionSettings.ConnectionPool.Nodes.FirstOrDefault();

                var settingsTemp =
                new ConnectionSettings(node?.Uri)
                .ApiKeyAuthentication(new Elasticsearch.Net.ApiKeyAuthenticationCredentials(_elasticClient.ConnectionSettings.ApiKeyAuthenticationCredentials.Base64EncodedApiKey))
                .EnableApiVersioningHeader();
                settingsTemp.DefaultIndex(tempIndexName);
                var clientTemp = new ElasticClient(settingsTemp);

                //delete temp index                
                var deleteTempIndexResponse = _elasticClient.Indices.Delete(Indices.Index(tempIndexName));
                if (!deleteTempIndexResponse.IsValid)
                {
                    _logger.LogError(deleteTempIndexResponse.OriginalException, deleteTempIndexResponse.DebugInformation);
                }

                //create temp index  
                var createTempIndexResponse = clientTemp.Indices.Create(Indices.Index(tempIndexName), i =>
                                                                 i.Settings(settings => settings
                                                                    .Analysis(analysis => analysis
                                                                        .Analyzers(analyzers => analyzers
                                                                               .Custom("text_analyzer", cust => cust
                                                                                        .Tokenizer("standard")
                                                                                        .Filters("lowercase", "asciifolding"))))));
                //.Map<DBBase<TEnt>>(m => m.AutoMap()));
                //.Map<DBBase<TEnt>>(m => m.AutoMap<DBBase<TEnt>>().Properties<DBBase<TEnt>>(p => p.Text(t => t.Name(n => n.Data).Analyzer("folding")))));

                //close temp index
                var closeResponseTempIndex = clientTemp.Indices.Close(Indices.Index(tempIndexName));

                if (!closeResponseTempIndex.IsValid)
                {
                    _logger.LogError(closeResponseTempIndex.OriginalException, closeResponseTempIndex.DebugInformation);
                    throw closeResponseTempIndex.OriginalException;
                }

                var getMappingResponseTemp = _elasticClient.Indices.GetMapping<DBBase<TEnt>>();
                var propertiesTemp = getMappingResponseTemp.Indices[Indices.Index(indexName)].Mappings.Properties;
                //var properties = new PropertyWalker(typeof(DBBase<TEnt>), null).GetProperties();
                SetTextAnalyzer("text_analyzer", propertiesTemp);

                var requestTemp = new PutMappingRequest<DBBase<TEnt>>()
                {
                    Properties = propertiesTemp
                };

                var putTempMappingResponse = clientTemp.Indices.PutMapping(requestTemp);

                if (!putTempMappingResponse.IsValid)
                {
                    _logger.LogError(putTempMappingResponse.OriginalException, putTempMappingResponse.DebugInformation);
                    throw putTempMappingResponse.OriginalException;
                }

                var openTempResponse = clientTemp.Indices.Open(Indices.Index(tempIndexName));
                if (!openTempResponse.IsValid)
                {
                    _logger.LogError(openTempResponse.OriginalException, openTempResponse.DebugInformation);
                    throw openTempResponse.OriginalException;
                }

                //reindex
                var reindexTempResponse = clientTemp.ReindexOnServer(r => r
                .Source(sou => sou.Index(indexName))
                .Destination(des => des.Index(tempIndexName))
                .WaitForCompletion(true)
                );

                if (!reindexTempResponse.IsValid)
                {
                    _logger.LogError(reindexTempResponse.OriginalException, reindexTempResponse.DebugInformation);
                    throw reindexTempResponse.OriginalException;
                }

                //delete index                
                var deleteIndexResponse = _elasticClient.Indices.Delete(Indices.Index(indexName));
                if (!deleteIndexResponse.IsValid)
                {
                    _logger.LogError(deleteIndexResponse.OriginalException, deleteIndexResponse.DebugInformation);
                    throw deleteIndexResponse.OriginalException;
                }

                //create index  
                var createIndexResponse = _elasticClient.Indices.Create(Indices.Index(indexName), i =>
                                                                     i.Settings(settings => settings
                                                                    .Analysis(analysis => analysis
                                                                        .Analyzers(analyzers => analyzers
                                                                               .Custom("text_analyzer", cust => cust
                                                                                        .Tokenizer("standard")
                                                                                        .Filters("lowercase", "asciifolding"))))));
                //.Map<DBBase<TEnt>>(m => m.AutoMap()));
                //.Map<DBBase<TEnt>>(m => m.AutoMap<DBBase<TEnt>>().Properties<DBBase<TEnt>>(p => p.Text(t => t.Name(n => n.Data).Analyzer("folding")))));

                //close index
                var closeResponseIndex = _elasticClient.Indices.Close(Indices.Index(indexName));

                if (!closeResponseIndex.IsValid)
                {
                    _logger.LogError(closeResponseIndex.OriginalException, closeResponseIndex.DebugInformation);
                    throw closeResponseIndex.OriginalException;
                }

                var getMappingResponseIndex = clientTemp.Indices.GetMapping<DBBase<TEnt>>();
                var propertiesIndex = getMappingResponseIndex.Indices[Indices.Index(tempIndexName)].Mappings.Properties;
                //var properties = new PropertyWalker(typeof(DBBase<TEnt>), null).GetProperties();
                SetTextAnalyzer("text_analyzer", propertiesTemp);

                //put index mapping
                var requestIndex = new PutMappingRequest<DBBase<TEnt>>()
                {
                    Properties = propertiesIndex
                };

                var putIndexMappingResponse = _elasticClient.Indices.PutMapping(requestIndex);

                if (!putIndexMappingResponse.IsValid)
                {
                    _logger.LogError(putIndexMappingResponse.OriginalException, putIndexMappingResponse.DebugInformation);
                    throw putIndexMappingResponse.OriginalException;
                }
                //open index
                var openIndexResponse = _elasticClient.Indices.Open(Indices.Index(indexName));
                if (!openIndexResponse.IsValid)
                {
                    _logger.LogError(openIndexResponse.OriginalException, openIndexResponse.DebugInformation);
                    throw openIndexResponse.OriginalException;
                }

                //reindex
                var reindexResponse = _elasticClient.ReindexOnServer(r => r
                .Source(sou => sou.Index(tempIndexName))
                .Destination(des => des.Index(indexName))
                .WaitForCompletion(true)
                );

                if (!reindexResponse.IsValid)
                {
                    _logger.LogError(reindexResponse.OriginalException, reindexResponse.DebugInformation);
                    throw reindexResponse.OriginalException;
                }

                //delete temp index                
                deleteTempIndexResponse = _elasticClient.Indices.Delete(Indices.Index(tempIndexName));
                if (!deleteTempIndexResponse.IsValid)
                {
                    _logger.LogError(deleteTempIndexResponse.OriginalException, deleteTempIndexResponse.DebugInformation);
                    throw deleteTempIndexResponse.OriginalException;
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message, exc);
                throw;
            }
            finally
            {
                var openResponse = _elasticClient.Indices.Open(Indices.Index(indexName));
                if (!openResponse.IsValid)
                {
                    _logger.LogError(openResponse.OriginalException, openResponse.DebugInformation);
                    throw openResponse.OriginalException;
                }
            }
        }

        #region private 

        private void SetTextAnalyzer(string analyzer, IProperties properties)
        {
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    if (property.Value.Type == "object")
                    {
                        var propertyDataObject = (ObjectProperty)property.Value;
                        SetTextAnalyzer(analyzer, propertyDataObject.Properties);
                    }
                    else if (property.Value.Type == "text")
                    {
                        var propertyDataItemText = (TextProperty)property.Value;
                        if (string.IsNullOrEmpty(propertyDataItemText.Analyzer))
                        {
                            propertyDataItemText.Analyzer = analyzer;
                        }
                    }
                }
            }
        }

        #endregion
    }
}

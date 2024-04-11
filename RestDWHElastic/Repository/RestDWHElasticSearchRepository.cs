using Microsoft.AspNetCore.JsonPatch;
using Nest;
using RestDWH.Base.Extensions;
using RestDWH.Base.Model;
using RestDWH.Base.Repository;
using System.Reflection;
using System.Security.Claims;

namespace RestDWH.Elastic.Repository
{
    public class RestDWHElasticSearchRepository<TEnt> : IDWHRepository<TEnt>
        where TEnt : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly RestDWHEvents<TEnt> _events;
        private readonly ILogger<RestDWHElasticSearchRepository<TEnt>> _logger;

        public RestDWHElasticSearchRepository(IElasticClient elasticClient, RestDWHEvents<TEnt> events, ILogger<RestDWHElasticSearchRepository<TEnt>> logger)
        {
            _elasticClient = elasticClient;
            _events = events;
            _logger = logger;
        }

        public virtual async Task<DBListBase<TEnt, DBBase<TEnt>>> GetAsync(int offset = 0, int limit = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null)
        {

            (offset, limit, query, sort) = await _events.BeforeGetAsync(offset, limit, query, sort, user);


            // Define the search request
            //var searchRequest = new SearchRequest<DBBase<TEnt>>
            //{
            //    Query = new QueryStringQuery
            //    {
            //        Query = query
            //    },

            //};
            //searchRequest.From = offset;
            //searchRequest.Size = limit;

            //var searchResponse = await _elasticClient.SearchAsync<DBBase<TEnt>>(searchRequest);

            var searchResponse = await _elasticClient.SearchAsync<DBBase<TEnt>>(s =>
            {
                s = s.From(offset);
                s = s.Size(limit);
                s = s.QueryOnQueryString(query);

                if (!string.IsNullOrEmpty(sort.Trim()))
                {
                    s = s.Sort(sortDescriptor =>
                    {

                        foreach (var sortItem in sort.Split(","))
                        {
                            var sortItemLower = sortItem.ToLower().Trim();
                            if (string.IsNullOrEmpty(sortItemLower.Trim())) continue;
                            var dir = sortItemLower.EndsWith(" desc") ? "desc" : sortItemLower.EndsWith(" asc") ? "asc" : "asc";
                            var strDir = $" {dir}";
                            var field = (sortItemLower.EndsWith(strDir) ? sortItem.Substring(0, sortItem.Length - strDir.Length) : sortItem).Trim();
                            if (dir == "asc")
                            {
                                sortDescriptor = sortDescriptor.Ascending(field);
                            }
                            else
                            {
                                sortDescriptor = sortDescriptor.Descending(field);
                            }
                        }

                        return sortDescriptor;
                    });
                }
                return s;
            });
            if (!string.IsNullOrEmpty(searchResponse.OriginalException?.Message)) throw new Exception(searchResponse.OriginalException?.Message);

            var count = await _elasticClient.CountAsync<DBBase<TEnt>>(s => s
                //.Index("person-main")
                .QueryOnQueryString(query)
                );

            var list = searchResponse.Hits.Select(s => { s.Source.Id = s.Id; return s.Source; }).ToArray();
            var instance = Activator.CreateInstance(typeof(DBListBase<TEnt, DBBase<TEnt>>)) as DBListBase<TEnt, DBBase<TEnt>>;
            if (instance == null) throw new Exception("Unable to inicialize DBListBase<TEnt, DBBase<TEnt>>");
            instance.Results = list;
            instance.Offset = offset;
            instance.Limit = limit;
            instance.TotalCount = count.Count;
            var result = await _events.AfterGetAsync(instance, offset, limit, query, sort, user);
            return result;
        }

        public virtual async Task<FieldsListBase> GetWithFieldsAsync(string fields = "id", int offset = 0, int limit = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null)
        {
            var data = await GetAsync(offset, limit, query, sort, user);
            var ret = new FieldsListBase() { From = data.Offset, Size = data.Limit, TotalCount = data.TotalCount };
            var list = new List<Dictionary<string, object>>();
            var fieldsStruct = ParseFields(fields);
            foreach (var item in data.Results)
            {
                var itemRet = new Dictionary<string, object>();
                foreach (var fieldKV in fieldsStruct)
                {
                    var val = GetObjectValue(fieldKV.Value, item);
                    itemRet = StoreValue(itemRet, fieldKV.Key, val);
                }
                list.Add(itemRet);
            }
            ret.Results = list;
            ret = await _events.AfterGetWithFieldsAsync(ret, fields, offset, limit, query, sort, user);
            return ret;
        }

        public virtual async Task<DBBase<TEnt>?> GetByIdAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            try
            {

                id = await _events.BeforeGetByIdAsync(id, user);
                var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);//
                if (!string.IsNullOrEmpty(searchResponse.OriginalException?.Message)) throw new Exception(searchResponse.OriginalException?.Message);
                if (searchResponse.Source == null)
                {
                    return null;
                }
                searchResponse.Source.Id = searchResponse.Id;
                var result = await _events.AfterGetByIdAsync(searchResponse.Source, id, user);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById error: {ex.Message}", ex);
                throw;
            }
        }

        public async Task<List<object>?> GetPropertiesAsync(string attribute, int offset = 0, int limit = 10, string query = "*", string sortType = "CountDescending", ClaimsPrincipal? user = null)
        {
            try
            {
                var searchResponse = await _elasticClient.SearchAsync<DBBase<TEnt>>(s =>
                {
                    s = s.QueryOnQueryString(query);// 
                    s = s.Aggregations(a => a.Terms(attribute, c =>
                    {
                        var ret = c.Field(new Field(attribute)).Size(limit);
                        if (sortType == "CountAscending") ret = ret.Order(o => o.CountAscending());
                        if (sortType == "CountDescending") ret = ret.Order(o => o.CountDescending());
                        if (sortType == "KeyAscending") ret = ret.Order(o => o.KeyAscending());
                        if (sortType == "KeyDescending") ret = ret.Order(o => o.KeyDescending());
                        if (sortType == "Ascending:") ret = ret.Order(o => o.Ascending(sortType.Replace("Ascending:", "")));
                        if (sortType == "Descending:") ret = ret.Order(o => o.Descending(sortType.Replace("Descending:", "")));
                        return ret;
                    }));
                    return s;
                });

                if (!string.IsNullOrEmpty(searchResponse.OriginalException?.Message)) throw new Exception(searchResponse.OriginalException?.Message);
                if (!searchResponse.Aggregations.Any()) throw new Exception("No aggregations");
                var result = searchResponse.Aggregations.Terms(attribute).Buckets.Select(s => s.Key as object).Skip(offset).Take(limit).ToList();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById error: {ex.Message}", ex);
                throw;
            }
        }

        public virtual async Task<Dictionary<string, object>> GetByIdWithFieldsAsync(string id, string fields = "id", System.Security.Claims.ClaimsPrincipal? user = null)
        {

            id = await _events.BeforeGetByIdAsync(id, user);
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);//
            if (!string.IsNullOrEmpty(searchResponse.OriginalException?.Message)) throw new Exception(searchResponse.OriginalException?.Message);
            if (searchResponse.Source == null) { throw new Exception("Not found"); }
            searchResponse.Source.Id = searchResponse.Id;
            var result = await _events.AfterGetByIdAsync(searchResponse.Source, id, user);
            var mapResult = MapEntityToFields(fields, result);
            mapResult = await _events.AfterGetByIdWithFieldsAsync(mapResult, fields, id, user);
            return mapResult;
        }

        public Dictionary<string, object> MapEntityToFields(string fields, DBBase<TEnt> obj)
        {
            var ret = new Dictionary<string, object>();
            foreach (var fieldKV in ParseFields(fields))
            {
                var val = GetObjectValue(fieldKV.Value, obj);
                ret = StoreValue(ret, fieldKV.Key, val);
            }
            return ret;
        }
        public Dictionary<string, object> StoreValue(Dictionary<string, object> ret, string field, object value)
        {
            var dict = ret;
            var routes = field.Split(".");
            var current = 0;
            foreach (var route in routes)
            {
                current++;
                if (current == routes.Length)
                {
                    // store value
                    dict[route] = value;
                }
                else
                {
                    if (dict.ContainsKey(route))
                    {
                        var newDict = dict[route] as Dictionary<string, object>;
                        if (newDict == null) { throw new Exception($"Error in fields structure. Field {route} has multiple types"); }
                        dict = newDict;
                    }
                    else
                    {
                        var newDict = new Dictionary<string, object>();
                        dict[route] = newDict;
                        dict = newDict;
                    }
                }
            }
            return ret;
        }
        public object GetObjectValue(string field, DBBase<TEnt> obj)
        {
            if (string.IsNullOrEmpty(field))
            {
                throw new ArgumentException($"'{nameof(field)}' cannot be null or empty.", nameof(field));
            }

            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            object ret = obj;
            foreach (var route in field.Split("."))
            {
                ret = ret?.GetType()?.GetProperty(route, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.GetValue(ret, null) ?? null;
            }
            return ret;
        }
        public Dictionary<string, string> ParseFields(string fields)
        {
            var list = new Dictionary<string, string>();
            foreach (var field in fields.Split(','))
            {
                var delim = field.IndexOf(":");
                if (delim == 0) { throw new Exception("Error parsing fields. Field cannot start with :"); }
                if (delim > 0)
                {
                    var first = field[..delim].Trim();
                    var second = field[(delim + 1)..].Trim();
                    if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) throw new Exception("Error parsing fields. Field cannot be empty.");
                    list[second] = first;
                }
                else
                {
                    var first = field.Trim();
                    list[first] = first;
                }
            }
            return list;
        }
        public virtual async Task<DBBase<TEnt>> PostAsync(TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            data = await _events.BeforePostAsync(data, user);
            var now = DateTimeOffset.Now;
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Created = now;
            instance.Updated = now;
            instance.Data = data;
            instance.CreatedBy = user?.Identity?.Name;
            instance.UpdatedBy = user?.Identity?.Name;
            instance = await _events.ToCreate(instance, user);
            var indexResponse = await _elasticClient.IndexDocumentAsync(instance);
            if (!indexResponse.IsValid) throw new Exception(indexResponse.DebugInformation);
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(indexResponse.Id);
            searchResponse.Source.Id = indexResponse.Id;
            var result = await _events.AfterPostAsync(searchResponse.Source, data, user);
            return result;
        }
        public virtual async Task<DBBase<TEnt>> PutAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            (id, data) = await _events.BeforePutAsync(id, data, user);
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            if (data?.Equals(searchResponse.Source.Data) == true)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Id = id;
            instance.Created = searchResponse.Source.Created;
            instance.Updated = DateTimeOffset.Now;
            instance.Data = data;
            instance.CreatedBy = searchResponse.Source.CreatedBy ?? user?.Identity?.Name;
            instance.UpdatedBy = user?.Identity?.Name;

            var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;
            if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = DateTimeOffset.Now;
            instanceLog.Data = searchResponse.Source.Data;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;
            (instance, instanceLog) = await _events.ToUpdate(instance, instanceLog, user);
            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            var result = await _events.AfterPutAsync(finalResponse.Source, id, data, user);
            return result;
        }

        public virtual async Task<DBBase<TEnt>> UpsertAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            (id, data) = await _events.BeforeUpsertAsync(id, data, user);
            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (searchResponse.Source != null && data?.Equals(searchResponse.Source.Data) == true)
            {
                return searchResponse.Source;// do not update if the docs are equal
            }
            var now = DateTimeOffset.Now;
            var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
            if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
            instance.Id = id;
            instance.Created = searchResponse.Source?.Created ?? now;
            instance.Updated = now;
            instance.Data = data;
            instance.CreatedBy = searchResponse.Source?.CreatedBy ?? user?.Identity?.Name;
            instance.UpdatedBy = user?.Identity?.Name;
            if (searchResponse.Source == null)
            {
                // new record
                instance = await _events.ToCreate(instance, user);
                _ = await _elasticClient.IndexDocumentAsync(instance);
            }
            else
            {
                // update record
                var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;
                if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
                instanceLog.Created = searchResponse.Source?.Created ?? now;
                instanceLog.Updated = now;
                instanceLog.Data = searchResponse.Source?.Data;
                instanceLog.UpdatedBy = searchResponse.Source?.UpdatedBy;
                instanceLog.RefId = searchResponse.Id;
                instanceLog.Version = searchResponse.Version;
                (instance, instanceLog) = await _events.ToUpdate(instance, instanceLog, user);
                _ = await _elasticClient.BulkAsync(r =>
                    r.
                    Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                    Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));
            }

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            var result = await _events.AfterUpsertAsync(finalResponse.Source, id, data, user);
            return result;
        }


        public virtual async Task<DBBase<TEnt>> PatchAsync(string id, JsonPatchDocument<TEnt> data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            try
            {
                (id, data) = await _events.BeforePatchAsync(id, data, user);
                var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
                if (searchResponse.Source == null || searchResponse.Source.Data == null)
                {
                    throw new Exception("Not found");
                }
                var orig = searchResponse.Source.Data.DeepCopy();

                data.ApplyTo(searchResponse.Source.Data);

                if (orig.Equals(searchResponse.Source.Data))
                {
                    return searchResponse.Source;// do not update if the docs are equal
                }
                var instance = Activator.CreateInstance(typeof(DBBase<TEnt>)) as DBBase<TEnt>;
                if (instance == null) throw new Exception("Unable to inicialize DBBase<TEnt>");
                instance.Created = searchResponse.Source.Created;
                instance.Updated = DateTimeOffset.Now;
                instance.Data = searchResponse.Source.Data;
                instance.CreatedBy = searchResponse.Source?.CreatedBy;
                instance.UpdatedBy = user?.Identity?.Name;

                var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;
                if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
                instanceLog.Created = searchResponse.Source.Created;
                instanceLog.Updated = DateTimeOffset.Now;
                instanceLog.Data = orig;
                instanceLog.CreatedBy = searchResponse.Source?.CreatedBy;
                instanceLog.UpdatedBy = searchResponse.Source?.UpdatedBy;
                instanceLog.RefId = searchResponse.Id;
                instanceLog.Version = searchResponse.Version;
                (instance, instanceLog) = await _events.ToUpdate(instance, instanceLog, user);
                var updateResponse = await _elasticClient.BulkAsync(r =>
                    r.
                    Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                    Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));

                var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
                if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
                finalResponse.Source.Id = finalResponse.Id;
                var result = await _events.AfterPatchAsync(finalResponse.Source, id, data, user);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message, exc);
                throw;
            }
        }
        public virtual async Task<DBBase<TEnt>> DeleteAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            //var deleteResponse = await _elasticClient.DeleteAsync<DBPerson>(id);
            id = await _events.BeforeDeleteAsync(id, user);

            var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (!searchResponse.IsValid)
            {
                throw new Exception("Not found");
            }
            var instanceLog = Activator.CreateInstance(typeof(DBBaseLog<TEnt>)) as DBBaseLog<TEnt>;

            if (instanceLog == null) throw new Exception("Unable to inicialize DBBaseLog<TEnt>");
            instanceLog.Created = searchResponse.Source.Created;
            instanceLog.Updated = searchResponse.Source.Updated;
            instanceLog.Deleted = DateTimeOffset.Now;
            instanceLog.Data = searchResponse.Source.Data;
            instanceLog.CreatedBy = searchResponse.Source.CreatedBy;
            instanceLog.UpdatedBy = searchResponse.Source.UpdatedBy;
            instanceLog.DeletedBy = user?.Identity?.Name;
            instanceLog.RefId = searchResponse.Id;
            instanceLog.Version = searchResponse.Version;

            instanceLog = await _events.ToDelete(instanceLog, user);

            var updateResponse = await _elasticClient.BulkAsync(r =>
                r.
                Index<DBBaseLog<TEnt>>(r => r.Document(instanceLog)).
                Delete<DBBase<TEnt>>(r => r.Id(id)));

            var errors = updateResponse.ItemsWithErrors.Select(e => e.Error?.Reason).Where(e => !string.IsNullOrEmpty(e));
            var hasErrors = errors?.Any() == true;
            if (hasErrors)
            {
                throw new Exception(string.Join(";", errors));
            }
            searchResponse.Source.Id = searchResponse.Id;
            var result = await _events.AfterDeleteAsync(searchResponse.Source, id, user);
            return result;
        }
    }
}
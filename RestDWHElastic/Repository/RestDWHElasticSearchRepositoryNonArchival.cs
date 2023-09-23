using Microsoft.AspNetCore.JsonPatch;
using Nest;
using RestDWH.Base.Extensions;
using RestDWH.Base.Model;
using RestDWH.Elastic.Repository;

namespace RestDWH.Elastic.Repository
{
    public class RestDWHElasticSearchRepositoryNonArchival<TEnt> : RestDWHElasticSearchRepository<TEnt> where TEnt : class
    {
        private readonly IElasticClient _elasticClient;
        private readonly RestDWHEvents<TEnt> _events;
        private readonly ILogger<RestDWHElasticSearchRepository<TEnt>> _logger;
        public RestDWHElasticSearchRepositoryNonArchival(IElasticClient elasticClient, RestDWHEvents<TEnt> events, ILogger<RestDWHElasticSearchRepository<TEnt>> logger) : base(elasticClient, events, logger)
        {
            _elasticClient = elasticClient;
            _events = events;
            _logger = logger;
        }

        public override async Task<DBBase<TEnt>> PutAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
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
                Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            var result = await _events.AfterPutAsync(finalResponse.Source, id, data, user);
            return result;
        }

        public override async Task<DBBase<TEnt>> UpsertAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null)
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
                    Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));
            }

            var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
            if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
            finalResponse.Source.Id = finalResponse.Id;
            var result = await _events.AfterUpsertAsync(finalResponse.Source, id, data, user);
            return result;
        }

        //private void jsonPatchError(JsonPatchError e)
        //{

        //}
        public override async Task<DBBase<TEnt>> PatchAsync(string id, JsonPatchDocument<TEnt> data, System.Security.Claims.ClaimsPrincipal? user = null)
        {
            try
            {
                (id, data) = await _events.BeforePatchAsync(id, data, user);
                var searchResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
                if (!searchResponse.IsValid || searchResponse.Source == null)
                {
                    throw new Exception("Not found");
                }
                if (searchResponse.Source == null || searchResponse.Source.Data == null)
                {
                    throw new Exception("Not found");
                }
                var orig = searchResponse.Source.Data.DeepCopy();
                var objData = searchResponse.Source.Data;
                //var ContractResolver = new DefaultContractResolver();
                //var adapter = new ObjectAdapter(ContractResolver, jsonPatchError);
                //foreach (var op in data.Operations)
                //{
                //    op.Apply(objData, adapter);
                //}
                data.ApplyTo(objData);

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
                    Update<DBBase<TEnt>>(r => r.Id(id).Doc(instance)));

                var finalResponse = await _elasticClient.GetAsync<DBBase<TEnt>>(id);
                if (finalResponse == null) throw new Exception($"FATAL Error occured. Failed to update {id} and instance is not available any more");
                finalResponse.Source.Id = finalResponse.Id;
                var result = await _events.AfterPatchAsync(finalResponse.Source, id, data, user);
                return result;
            }
            catch (Exception exc)
            {
                _logger.LogError($"Patch error: {exc.Message}", exc);
                throw;
            }
        }
        public override async Task<DBBase<TEnt>> DeleteAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null)
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

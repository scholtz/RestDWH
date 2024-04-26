using RestDWH.Base.Model;

namespace RestDWH.Elastic.Repository
{
    public interface IElasticDWHRepository<TEnt> where TEnt : class
    {
        public string GetMainIndex();
        public string GetLogIndex();
        public Task<DBListBase<TEnt, DBBase<TEnt>>> QueryAsync(string query, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<List<object>?> GetPropertiesAsync(string query, string attribute, int offset = 0, int limit = 10, System.Security.Claims.ClaimsPrincipal? user = null);
    }
}

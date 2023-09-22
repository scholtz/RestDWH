using Microsoft.AspNetCore.JsonPatch;
using RestDWH.Base.Model;

namespace RestDWH.Base.Repository
{
    public interface IDWHRepository<TEnt> where TEnt : class
    {
        public Task<DBListBase<TEnt, DBBase<TEnt>>> GetAsync(int offset = 0, int limit = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<FieldsListBase> GetWithFieldsAsync(string fields = "id", int offset = 0, int limit = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>?> GetByIdAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<List<object>?> GetPropertiesAsync(string attribute, int offset = 0, int limit = 10, string query = "*", string sortType = "CountDescending", System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<Dictionary<string, object>> GetByIdWithFieldsAsync(string id, string fields = "id", System.Security.Claims.ClaimsPrincipal? user = null);

        public Task<DBBase<TEnt>> PostAsync(TEnt data, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>> PutAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>> UpsertAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null);

        public Task<DBBase<TEnt>> PatchAsync(string id, JsonPatchDocument<TEnt> data, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>> DeleteAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null);
    }

}

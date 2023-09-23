using Microsoft.AspNetCore.JsonPatch;
using RestDWH.Model;

namespace RestDWH.Repository
{
    public interface IDWHRepository<TEnt> where TEnt : class
    {
        public Task<DBListBase<TEnt, DBBase<TEnt>>> GetAsync(int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<FieldsListBase> GetWithFieldsAsync(string fields = "id", int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>?> GetByIdAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<Dictionary<string, object>> GetByIdWithFieldsAsync(string id, string fields = "id", System.Security.Claims.ClaimsPrincipal? user = null);

        public Task<DBBase<TEnt>> PostAsync(TEnt data, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>> PutAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>> UpsertAsync(string id, TEnt data, System.Security.Claims.ClaimsPrincipal? user = null);

        public Task<DBBase<TEnt>> PatchAsync(string id, JsonPatchDocument<TEnt> data, System.Security.Claims.ClaimsPrincipal? user = null);
        public Task<DBBase<TEnt>> DeleteAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null);
    }

}

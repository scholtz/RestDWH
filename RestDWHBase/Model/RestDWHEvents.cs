using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;

namespace RestDWH.Base.Model
{
    public class RestDWHEvents<T>
        where T : class
    {
        /// <summary>
        /// Before each data request
        /// </summary>
        /// <param name="user"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public virtual async Task BeforeEachAsync(System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {

        }
        public virtual async Task<(int from, int size, string query, string sort)> BeforeGetAsync(int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return (from, size, query, sort);
        }
        public virtual async Task<DBListBase<T, DBBase<T>>> AfterGetAsync(DBListBase<T, DBBase<T>> result, int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }
        public virtual async Task<FieldsListBase> AfterGetWithFieldsAsync(FieldsListBase result, string fields, int from = 0, int size = 10, string query = "*", string sort = "", System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }

        public virtual async Task<string> BeforeGetByIdAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return id;
        }
        public virtual async Task<DBBase<T>> AfterGetByIdAsync(DBBase<T> result, string id, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }
        public virtual async Task<Dictionary<string, object>> AfterGetByIdWithFieldsAsync(Dictionary<string, object> result, string fields, string id, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }

        public virtual async Task<T> BeforePostAsync(T data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return data;
        }
        public virtual async Task<DBBase<T>> AfterPostAsync(DBBase<T> result, T data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }
        public virtual async Task<(string id, T data)> BeforePutAsync(string id, T data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return (id, data);
        }
        public virtual async Task<DBBase<T>> AfterPutAsync(DBBase<T> result, string id, T data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }

        public virtual async Task<(string id, T data)> BeforeUpsertAsync(string id, T data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return (id, data);
        }
        public virtual async Task<DBBase<T>> AfterUpsertAsync(DBBase<T> result, string id, T data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }
        public virtual async Task<(string id, JsonPatchDocument<T> data)> BeforePatchAsync(string id, JsonPatchDocument<T> data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return (id, data);
        }
        public virtual async Task<DBBase<T>> AfterPatchAsync(DBBase<T> result, string id, JsonPatchDocument<T> data, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }
        public virtual async Task<string> BeforeDeleteAsync(string id, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return id;
        }
        public virtual async Task<DBBase<T>> AfterDeleteAsync(DBBase<T> result, string id, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }



        public virtual async Task<DBBase<T>> ToCreate(DBBase<T> item, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return item;
        }

        public virtual async Task<(DBBase<T>, DBBaseLog<T>)> ToUpdate(DBBase<T> item, DBBaseLog<T> logItem, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return (item, logItem);
        }

        public virtual async Task<DBBaseLog<T>> ToDelete(DBBaseLog<T> logItem, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return logItem;
        }

    }
}

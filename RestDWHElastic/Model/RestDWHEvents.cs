using RestDWH.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestDWH.Elastic.Model
{
    public class RestDWHEventsElastic<T> : RestDWH.Base.Model.RestDWHEvents<T>
        where T : class
    {
        /// <summary>
        /// Before the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="user"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public virtual async Task<string> BeforeQueryAsync(string query = "", System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return query;
        }
        /// <summary>
        /// After query
        /// </summary>
        /// <param name="result"></param>
        /// <param name="query"></param>
        /// <param name="user"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public virtual async Task<DBListBase<T, DBBase<T>>> AfterQueryAsync(DBListBase<T, DBBase<T>> result, string query = "", System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return result;
        }
        /// <summary>
        /// Before get properties
        /// </summary>
        /// <param name="query"></param>
        /// <param name="attribute"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="user"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public virtual async Task<(string query, string attribute, int offset, int limit)> BeforeGetPropertiesAsync(string query, string attribute, int offset = 0, int limit = 10, System.Security.Claims.ClaimsPrincipal? user = null, IServiceProvider? serviceProvider = null)
        {
            return (query, attribute, offset, limit);
        }
    }
}

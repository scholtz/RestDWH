using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using RestDWH.Model;
using RestDWH.Repository;

namespace RestDWH.Controllers
{
    public class BaseController<TEnt> : ControllerBase
        where TEnt : class
    {
        private readonly BaseRepository<
            TEnt
        > repo;
        public BaseController(BaseRepository<
            TEnt
        > repo)
        {
            this.repo = repo;
        }
        /// <summary>
        /// Search item from elasticsearch db
        /// </summary>
        /// <param name="from">From record number</param>
        /// <param name="size">Number of records per page</param>
        /// <param name="query">Elastic search query - data.streetLine : "Dopravaku" or data.street : "s"</param>
        /// <param name="sort">Sort by columns. Separated by comma. 'created desc, updated asc'</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet($"v1/Get[controller]")]
        public Task<DBListBase<TEnt, DBBase<TEnt>>> Get(int from = 0, int size = 10, string query = "*", string sort = "")
        {
            return repo.Get(from, size, query, sort, User);
        }

        /// <summary>
        /// Search item from elasticsearch db
        /// </summary>
        /// <param name="from">From record number</param>
        /// <param name="size">Number of records per page</param>
        /// <param name="query">Elastic search query - data.streetLine : "Dopravaku" or data.street : "s"</param>
        /// <param name="sort">Sort by columns. Separated by comma. 'created desc, updated asc'</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet($"v1/Get[controller]WithFields")]
        public Task<FieldsListBase> GetWithFields(string fields = "id", int from = 0, int size = 10, string query = "*", string sort = "")
        {
            return repo.GetWithFields(fields, from, size, query, sort, User);
        }

        [Authorize]
        [HttpGet("v1/Get[controller]ById/{id}")]
        public Task<DBBase<TEnt>?> GetById(string id)
        {
            return repo.GetById(id, User);
        }
        /// <summary>
        /// Returns entity with limited fields only
        /// 
        /// fields attribute: [field]:[mapToKey],[field2]:[mapToKey2]
        /// 
        /// example:
        /// id,data.attribute:a .. returns dictionary [id=val,a=attribute]
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("v1/Get[controller]ByIdWithFields/{id}")]
        public Task<Dictionary<string,object>> GetByIdWithFields(string id, string fields = "id")
        {
            return repo.GetByIdWithFields(id, fields, User);
        }

        [Authorize]
        [HttpPost("v1/Post[controller]")]
        public Task<DBBase<TEnt>> Post([FromBody] TEnt data)
        {
            return repo.Post(data, User);
        }

        [Authorize]
        [HttpPut("v1/Put[controller]/{id}")]
        public Task<DBBase<TEnt>> Put([FromRoute] string id, [FromBody] TEnt data)
        {
            return repo.Put(id, data, User);
        }


        [Authorize]
        [HttpPut("v1/Upsert[controller]/{id}")]
        public Task<DBBase<TEnt>> Upsert([FromRoute] string id, [FromBody] TEnt data)
        {
            return repo.Upsert(id, data, User);
        }

        [Authorize]
        [HttpPatch("v1/Patch[controller]/{id}")]
        public Task<DBBase<TEnt>> Patch([FromRoute] string id, [FromBody] JsonPatchDocument<TEnt> data)
        {
            return repo.Patch(id, data, User);
        }

        [Authorize]
        [HttpDelete("v1/Delete[controller]/{id}")]
        public Task<DBBase<TEnt>> Delete(string id)
        {
            return repo.Delete(id, User);
        }
    }
}

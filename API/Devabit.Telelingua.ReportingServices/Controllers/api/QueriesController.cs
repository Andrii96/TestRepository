using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Devabit.Telelingua.ReportingServices.DataManagers.Interface;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Devabit.Telelingua.ReportingServices.Web.Controllers.api
{
    [Authorize(Roles = "Administrator, SeniorExecs")]
    [Route("api/[controller]")]
    public class QueriesController : Controller
    {
        private readonly IQueryDataManager dataManager;

        public QueriesController(IQueryDataManager dataManager)
        {
            this.dataManager = dataManager;
        }

        /// <summary>
        /// Adds new query.
        /// </summary>
        /// <param name="query">The query to be saved.</param>
        /// <returns>The query identifier.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public long Add([FromBody] QuerySaveModel query)
        {
            return this.dataManager.AddQuery(query);
        }

        /// <summary>
        /// Gets the list of queries.
        /// </summary>
        /// <returns>The list of saved queries.</returns>
        [HttpGet]
        public List<QueryViewModel> GetQueries()
        {
            var queries = this.dataManager.GetQueries();
           
            if (!User.IsInRole("Administrator"))
            {
                var connectionStringName = User.Claims.FirstOrDefault(x => x.Type == "Entity").Value.Substring(0, 3);
                var filteredResult = queries.Where(q =>
                {
                    var query = dataManager.GetInnerQuery(q.Id);
                    return query.SavedQueryTemplate.ConnectionStringName == connectionStringName;
                }).ToList();
                return filteredResult;
            }
            return queries;
        }

        /// <summary>
        /// Gets the query related data.
        /// </summary>
        /// <param name="id">The query identifier.</param>
        /// <returns>The saved query model.</returns>
        [HttpGet("{id}")]
        public SavedQueryModel GetQuery(long id)
        {
            return this.dataManager.GetQuery(id);
        }

        /// <summary>
        /// Gets sql script from model
        /// </summary>
        /// <param name="model"></param>
        /// <returns>sql script</returns>
        [HttpPost("sqlScriptFromQuery")]
        public string GetSqlScriptFromQuery([FromBody]PagedQueryModel model)
        {
            return dataManager.GetScriptFromQuery(model);
        }

        /// <summary>
        /// Processes the saved query.
        /// </summary>
        /// <param name="model">Model, containing query identifier and explicit filters values</param>
        /// <returns>The query processing result.</returns>
        [HttpPost("process")]
        public SavedQueryResult GetQueryResult([FromBody]SavedQueryProcessModel model)
        {
            if (!User.IsInRole("Administrator"))
            {
                model.ConnectionStringName = User.Claims.FirstOrDefault(x => x.Type == "Entity").Value.Substring(0, 3);
            }
            return this.dataManager.ProcessSavedQuery(model);
        }

        [HttpGet("inner/{id}")]
        public InnerQueryModel GetInnerQuery(long id)
        {
            return this.dataManager.GetInnerQuery(id);
        }

        /// <summary>
        /// Saves sql script representation instead of sql model
        /// </summary>
        /// <param name="id">saved sql template id</param>
        /// <param name="sqlScript">script representation of template</param>
        /// <returns>new id of query</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut("editQuerySqlScript/{id}")]
        public long EditQuerySqlScript(long id, [FromBody]string sqlScript)
        {
           return dataManager.EditQuerySqlScript(id, sqlScript);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public void EditQuery(long id, [FromBody]PagedQueryModel model)
        {
            this.dataManager.EditQuery(id, model);
        }

        /// <summary>
        /// Deletes query.
        /// </summary>
        /// <param name="id">The query identifier.</param>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public void DeleteQuery(long id)
        {
            this.dataManager.DeleteQuery(id);
        }

        [HttpPost("export")]
        public async Task Export([FromBody] SavedQueryProcessModel model)
        {
            if (!User.IsInRole("Administrator"))
            {
                model.ConnectionStringName = User.Claims.FirstOrDefault(x => x.Type == "Entity").Value.Substring(0, 3);
            }
            var data = dataManager.GetAllRecords(model);
            var bytesInStream = await ConvertResultToByteArray(data.QueryResult);
            if (bytesInStream.Length > 0)
            {
                HttpContext.Response.ContentType = "application/force-download";
                var name = $"attachment; filename=Data-{DateTime.UtcNow:s}.csv";
                HttpContext.Response.Headers.Add("content-disposition", name);
                await HttpContext.Response.Body.WriteAsync(bytesInStream, 0, bytesInStream.Length);
            }
        }

        [HttpPost("exportSqlScriptResult")]
        public async Task Export([FromBody] SavedScriptProcessModel model)
        {
            if (!User.IsInRole("Administrator"))
            {
                model.ConnectionStringName = User.Claims.FirstOrDefault(x => x.Type == "Entity").Value.Substring(0, 3);
            }
            var data = dataManager.ProcessSavedSqlScript(model,true);
            var bytesInStream = await ConvertResultToByteArray(data);
            if (bytesInStream.Length > 0)
            {
                HttpContext.Response.ContentType = "application/force-download";
                var name = $"attachment; filename=Data-{DateTime.UtcNow:s}.csv";
                HttpContext.Response.Headers.Add("content-disposition", name);
                await HttpContext.Response.Body.WriteAsync(bytesInStream, 0, bytesInStream.Length);
            }
        }
        
        #region Helpers
        private async Task<byte[]> ConvertResultToByteArray(PagedQueryResultModel data)
        {
            byte[] bytesInStream;
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter writer = new StreamWriter(memoryStream))
                {
                    await writer.WriteLineAsync(string.Join(";",
                        data.Result.ColumnHeaders.Select(c => c.Name)));
                    foreach (var resultRow in data.Result.Rows)
                        await writer.WriteLineAsync(string.Join(";", resultRow.Values));
                    await writer.FlushAsync();
                    bytesInStream = memoryStream.ToArray();
                }
            }
            return bytesInStream;
        }
        #endregion
    }
}

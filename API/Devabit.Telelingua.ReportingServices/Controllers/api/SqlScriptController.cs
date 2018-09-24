using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devabit.Telelingua.ReportingServices.DataManagers.Interface;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Devabit.Telelingua.ReportingServices.Web.Controllers.api
{
    
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [Authorize(Roles = "Administrator, SeniorExecs")]
    public class SqlScriptController : Controller
    {
        private readonly IQueryDataManager dataManager;

        public SqlScriptController(IQueryDataManager dataManager)
        {
            this.dataManager = dataManager;
        }

        /// <summary>
        /// Saves sql script to db
        /// </summary>
        /// <param name="script"> script to be saved</param>
        /// <returns> id of saved script</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public long AddScript([FromBody] SqlScriptSaveModel script)
        {
            return dataManager.AddSqlScript(script);
        }

        /// <summary>
        /// Gets list of saved sql scripts
        /// </summary>
        /// <returns> sql scripts list</returns>
        [HttpGet]
        public List<QueryViewModel> GetSqlScripts()
        {
            var scripts = dataManager.GetSqlScripts();

            if (!User.IsInRole("Administrator"))
            {
                var connectionStringName = User.Claims.FirstOrDefault(x => x.Type == "Entity").Value.Substring(0, 3);
                var filteredResult = scripts.Where(s =>
                {
                    var script = dataManager.GetSqlScript(s.Id);
                    return script.ConnectionStringName == connectionStringName;
                }).ToList();
                return filteredResult;
            }
            return scripts;
        }

        /// <summary>
        /// Gets sql script via id
        /// </summary>
        /// <param name="id">script id</param>
        /// <returns></returns>
        [HttpGet]
        public SavedScriptModel GetSqlScript(long id)
        {
            return dataManager.GetSqlScript(id);
        }

        /// <summary>
        /// Processes saved sql script
        /// </summary>
        /// <param name="id">Id of processing script</param>
        /// <returns></returns>
        [HttpPost]
        public PagedQueryResultModel ProcessSqlScript([FromBody]SavedScriptProcessModel script)
        {
            if (!User.IsInRole("Administrator"))
            {
                script.ConnectionStringName = User.Claims.FirstOrDefault(x => x.Type == "Entity").Value.Substring(0, 3);
            }
            return dataManager.ProcessSavedSqlScript(script);
        }

        [AllowAnonymous]
        [HttpPost]
        public List<string> GetParametersFromScript([FromBody]string sqlScript)
        {
            return dataManager.GetSqlParameters(sqlScript);
        }

        /// <summary>
        /// Deletes sql script from db
        /// </summary>
        /// <param name="id">Id of script to be deleted</param>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public void DeleteSqlScript(long id)
        {
            dataManager.DeleteSqlScript(id);
        }

        /// <summary>
        /// Edits existing sql script in db
        /// </summary>
        /// <param name="id">id of script to be updated</param>
        /// <param name="model">updated script</param>
        [Authorize(Roles = "Administrator")]
        [HttpPut("{id}")]
        public void EditSqlScript(long id,[FromBody] SqlRequestModel model)
        {
            dataManager.EditSqlScript(id, model);
        }

    }
}
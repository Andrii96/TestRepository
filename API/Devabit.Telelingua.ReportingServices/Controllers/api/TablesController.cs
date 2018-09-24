using System.Collections.Generic;
using Devabit.Telelingua.ReportingServices.DataManagers.Interface;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Devabit.Telelingua.ReportingServices.Web.Controllers.api
{
    [Authorize]
    [Route("api/[controller]")]
    public class TablesController : Controller
    {
        private readonly ITableDataManager manager;

        public TablesController(ITableDataManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Gets the list of table names.
        /// </summary>
        /// <returns>The list of table names.</returns>
        [HttpGet]
        public IEnumerable<TableViewModel> Get()
        {
            return manager.GetTableNames();
        }

        /// <summary>
        /// Gets the table definition.
        /// </summary>
        /// <param name="table">Model that specifies the table.</param>
        /// <returns>The table schema model.</returns>
        [HttpGet("table")]
        public TableDataModel Get([FromQuery]TableViewModel table)
        {
             return manager.GetTable(table);
        }

        /// <summary>
        /// Processes the query.
        /// </summary>
        /// <param name="querie">The query to be processed.</param>
        /// <returns>The query result model.</returns>
        [HttpPost("query")]
        public PagedQueryResultModel ProcessQuery([FromBody]PagedQueryModel querie)
        {
            //if (querie == null)
            //{
            //    var json = System.IO.File.ReadAllText("./test.txt");
            //    querie = JsonConvert.DeserializeObject<PagedQueryModel>(json);
            //}
            return manager.ProcessQuery(querie);
        }

        [HttpPost("processSqlScript")]
        public PagedQueryResultModel ProcessSqlScript([FromBody] SqlScriptModel querie)
        {
            return manager.ProcessSqlScript(querie);
        }
        
    }
}

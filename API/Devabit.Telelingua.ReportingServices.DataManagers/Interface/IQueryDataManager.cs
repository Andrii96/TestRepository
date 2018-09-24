using System;
using System.Collections.Generic;
using System.Text;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.DataManagers.Interface
{
    public interface IQueryDataManager
    {
        /// <summary>
        /// Adds new saved query.
        /// </summary>
        /// <param name="query">The saved query model.</param>
        /// <returns>The query identifier.</returns>
        long AddQuery(QuerySaveModel query);

        /// <summary>
        /// Gets sql script representation of query model
        /// </summary>
        /// <param name="query"> model</param>
        /// <returns>sql script</returns>
        string GetScriptFromQuery(PagedQueryModel query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sqlScript"></param>
        /// <returns></returns>
        long EditQuerySqlScript(long id, string sqlScript);

        long AddSqlScript(SqlScriptSaveModel script);

        /// <summary>
        /// Gets all saved queries.
        /// </summary>
        /// <returns>The list of saved queries.</returns>
        List<QueryViewModel> GetQueries();

        List<string> GetSqlParameters(string sqlScript);
        List<QueryViewModel> GetSqlScripts();
        /// <summary>
        /// Gets the query related data.
        /// </summary>
        /// <param name="id">The query identifier.</param>
        /// <returns>The saved query model.</returns>
        SavedQueryModel GetQuery(long id);

        SavedScriptModel GetSqlScript(long id);
        /// <summary>
        /// Processes the saved query.
        /// </summary>
        /// <param name="model">The model containing identifier and explicit filters values.</param>
        /// <returns>The result of query processing.</returns>
        SavedQueryResult ProcessSavedQuery(SavedQueryProcessModel model);

        PagedQueryResultModel ProcessSavedSqlScript(SavedScriptProcessModel script, bool getAllRecords = false);
        /// <summary>
        /// Gets all records for provided query.
        /// </summary>
        /// <param name="model">Modle to process.</param>
        /// <returns>The result of query processing.</returns>
        SavedQueryResult GetAllRecords(SavedQueryProcessModel model);

        /// <summary>
        /// Deletes query.
        /// </summary>
        /// <param name="id">The query identifier.</param>
        void DeleteQuery(long id);

        void DeleteSqlScript(long id);
        /// <summary>
        /// Gets the template query model.
        /// </summary>
        /// <param name="id">Query identifier</param>
        /// <returns>The query</returns>
        InnerQueryModel GetInnerQuery(long id);

        /// <summary>
        /// Edits query in db.
        /// </summary>
        /// <param name="id">Query identifier.</param>
        /// <param name="model">Query model.</param>
        void EditQuery(long id, PagedQueryModel model);

        void EditSqlScript(long id, SqlRequestModel model);
    }
}

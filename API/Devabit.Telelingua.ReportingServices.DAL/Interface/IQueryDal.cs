using System.Collections.Generic;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.DAL.Interface
{
    public interface IQueryDal
    {
        /// <summary>
        /// Adds new saved query.
        /// </summary>
        /// <param name="query">The saved query model.</param>
        /// <returns>The query identifier.</returns>
        long AddQuery(QuerySaveModel query);

        long EditQuerySqlScript(long id, string sqlScript);

        long AddSqlScript(SqlScriptSaveModel sqlScript);

        /// <summary>
        /// Gets all saved queries.
        /// </summary>
        /// <returns>The list of saved queries.</returns>
        List<QueryViewModel> GetQueries();

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
        /// <param name="id">The query identifier.</param>
        /// <param name="filters">List of explicit filters values.</param>
        /// <returns>The result of query processing.</returns>
        SavedQueryResult ProcessSavedQuery(long id, List<ExplicitFilterModel> filters);

        SqlRequestModel ProcessSavedSqlScript(long id);
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

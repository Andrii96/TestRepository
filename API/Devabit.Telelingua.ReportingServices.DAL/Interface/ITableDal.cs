using System.Collections.Generic;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.DAL.Interface
{
    /// <summary>
    /// The interface for tables dal.
    /// </summary>
    public interface ITableDal
    {
        /// <summary>
        /// Gets the list of tables names.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TableViewModel> GetTableNames();

        /// <summary>
        /// Gets info about specified table
        /// </summary>
        /// <param name="tableName">Table model representing table`s name</param>
        /// <returns>Corresponding table view model.</returns>
        TableDataModel GetTable(TableViewModel tableName);

        string GetSqlScriptFromQueryModel(PagedQueryModel query);
        int GetRecordsCount(QueryModel query, string connectionStringName = null);

        /// <summary>
        /// Processes the query.
        /// </summary>
        /// <param name="query">The query to be processed.</param>
        /// <param name="comparisons">The comparisons to get.</param>
        /// <param name="externalCalculations">The external calculations to be performed on final dataset.</param>
        /// <returns>The query result.</returns>
        PagedQueryResultModel ProcessQuery(PagedQueryModel query,
             string connectionStringName = null,
            List<ComparableFieldModel> comparisons = null,
            List<CalculatedColumnModel> externalCalculations = null);

        PagedQueryResultModel ProcessSqlScript(SqlScriptModel sqlRequest);
    }
}

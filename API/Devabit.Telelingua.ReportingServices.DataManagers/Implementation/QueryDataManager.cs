using System.Collections.Generic;
using System.Linq;
using Devabit.Telelingua.ReportingServices.DataManagers.Interface;
using Devabit.Telelingua.ReportingServices.DAL.Interface;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Devabit.Telelingua.ReportingServices.Enums;
using System.Text.RegularExpressions;
using Devabit.Telelingua.ReportingServices.Helpers;

namespace Devabit.Telelingua.ReportingServices.DataManagers.Implementation
{
    public class QueryDataManager : IQueryDataManager
    {
        private readonly IQueryDal dal;

        private readonly ITableDal tableDal;

        private readonly ITableDataManager tableDataManager;

        public QueryDataManager(IQueryDal dal, ITableDal tableDal, ITableDataManager tableDataManager)
        {
            this.dal = dal;
            this.tableDal = tableDal;
            this.tableDataManager = tableDataManager;
        }

        ///<inheritdoc/>
        public long AddQuery(QuerySaveModel query)
        {
            return this.dal.AddQuery(query);
        }

        public long AddSqlScript(SqlScriptSaveModel script)
        {
            return dal.AddSqlScript(script);
        }

        public List<string> GetSqlParameters(string sqlScript)
        {
            var parameters = new List<string>();
            Regex regex = new Regex(@"(.*?)\[@(\w+)@\]");
            var match = regex.Match(sqlScript);
            while (match.Success)
            {
                var parameterName = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(parameterName))
                {
                    if (!parameters.Contains(parameterName))
                    {
                        parameters.Add(parameterName);
                    } 
                }
                match = match.NextMatch();
            }

            return parameters;
        }
        ///<inheritdoc/>
        public List<QueryViewModel> GetQueries()
        {
            return this.dal.GetQueries();
        }

        public List<QueryViewModel> GetSqlScripts()
        {
            return dal.GetSqlScripts();
        }

        ///<inheritdoc/>
        public SavedQueryModel GetQuery(long id)
        {
            var query = this.dal.GetQuery(id);
            this.PrepareExplicitFilters(query);
            return query;
        }

        public string GetScriptFromQuery(PagedQueryModel query)
        {
            var sqlScript = tableDal.GetSqlScriptFromQueryModel(query);
            return sqlScript;
        }

        public SavedScriptModel GetSqlScript(long id)
        {
            return dal.GetSqlScript(id);
        }

        public long EditQuerySqlScript(long id, string sqlScript)
        {
            return dal.EditQuerySqlScript(id, sqlScript);
        }
        ///<inheritdoc/>
        public SavedQueryResult ProcessSavedQuery(SavedQueryProcessModel model)
        {
            var res = this.dal.ProcessSavedQuery(model.Id, model.Filters);
            res.Query.PagingModel = model.Pagination;
            res.Query.ConnectionStringName = model.ConnectionStringName;
            this.PrepareDateFilters(model.Comparisons);

            res.QueryResult =
                this.tableDataManager.ProcessQuery(res.Query, model.Comparisons, model.ExternalCalculations);
            return res;
        }

        public PagedQueryResultModel ProcessSavedSqlScript(SavedScriptProcessModel script, bool getAllRecords = false)
        {
            var savedScript = dal.ProcessSavedSqlScript(script.Id);
            savedScript.PaginationModel = script.Pagination;
            if (getAllRecords)
            {
                savedScript.PaginationModel.PageSize = int.MaxValue;
                savedScript.PaginationModel.PageNumber = 1;
            }
            savedScript.ConnectionStringName = script.ConnectionStringName;
            var scriptModel = new SqlScriptModel
            {
                SqlRequest = savedScript,
                Parameters = script.Parameters
            };
            var result = tableDataManager.ProcessSqlScript(scriptModel);
            return result;
        }

        
        ///<inheritdoc/>
        public SavedQueryResult GetAllRecords(SavedQueryProcessModel model)
        {
            var res = this.dal.ProcessSavedQuery(model.Id, model.Filters);
            res.Query.PagingModel = model.Pagination;
            res.Query.ConnectionStringName = model.ConnectionStringName;
            res.Query.PagingModel.PageSize = int.MaxValue;
            res.Query.PagingModel.PageNumber = 1;
            this.PrepareDateFilters(model.Comparisons);

            res.QueryResult =
                this.tableDataManager.ProcessQuery(res.Query, model.Comparisons, model.ExternalCalculations);
            return res;
        }

        ///<inheritdoc/>
        public void DeleteQuery(long id)
        {
            this.dal.DeleteQuery(id);
        }

        public void DeleteSqlScript(long id)
        {
            dal.DeleteSqlScript(id);
        }

        public InnerQueryModel GetInnerQuery(long id)
        {
            return this.dal.GetInnerQuery(id);
        }

        public void EditQuery(long id, PagedQueryModel model)
        {
            this.dal.EditQuery(id, model);
        }


        public void EditSqlScript(long id,SqlRequestModel model)
        {
            dal.EditSqlScript(id, model);
        }
        /// <summary>
        /// Sets the types for explicit filters.
        /// </summary>
        /// <param name="model">The model to be prepared.</param>
        private void PrepareExplicitFilters(SavedQueryModel model)
        {
            var tables = model.Filters.Select(x => new { x.TableName, x.SchemaName }).Distinct();
            foreach (var table in tables)
            {
                var columns = tableDal.GetTable(new TableViewModel()
                {
                    Name = table.TableName,
                    Schema = table.SchemaName
                }).Columns;

                foreach (var filter in model.Filters.Where(x => x.TableName == table.TableName && x.SchemaName == table.SchemaName))
                {
                    filter.ColumnType = columns[filter.ColumnName];
                }
            }
        }

        private void PrepareDateFilters(List<ComparableFieldModel> comparisons)
        {
            foreach (var comparison in comparisons)
            {
                foreach (var filter in comparison.Filters)
                {
                    if (filter.ColumnType == ColumnTypes.Date && filter.Value.Length == 4)
                    {
                        filter.ColumnName = $"year({filter.ColumnName})";
                    }
                }
            }

        }

        
    }
}
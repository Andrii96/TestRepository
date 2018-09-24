using System.Collections.Generic;
using System.Linq;
using Devabit.Telelingua.ReportingServices.DataAccess;
using Devabit.Telelingua.ReportingServices.DataAccess.Models;
using Devabit.Telelingua.ReportingServices.DAL.Interface;
using Devabit.Telelingua.ReportingServices.Enums;
using Devabit.Telelingua.ReportingServices.Helpers;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Newtonsoft.Json;
using Devabit.Telelingua.ReportingServices.DAL.Helpers;

namespace Devabit.Telelingua.ReportingServices.DAL.Implementation
{
    public class QueryDal : IQueryDal
    {
        #region Fields

        private QueryDbContext Context { get; set; }

        #endregion

        #region Constructor

        ///<inheritdoc/>
        public QueryDal(QueryDbContext context)
        {
            this.Context = context;
        }

        #endregion

        #region  Query region

        ///<inheritdoc/>
        public long AddQuery(QuerySaveModel query)
        {
            var model = new Query()
            {
                Name = query.Name,
                QueryText = JsonConvert.SerializeObject(query.Query),
                SqlQuery = query.SqlQuery,
                IsSqlEdited = query.IsSqlChanged,
                CategoryId = query.CategoryId
            };
            Context.Queries.Add(model);

            Context.SaveChanges();
            return model.Id;
        }

        ///<inheritdoc/>
        public InnerQueryModel GetInnerQuery(long id)
        {
            var result = new InnerQueryModel();
            var query = Context.Queries.FirstOrDefault(x => x.Id == id);
            if (query != null)
            {
                result.SavedQueryTemplate = JsonConvert.DeserializeObject<PagedQueryModel>(query.QueryText);
                return result;
            }
            else
            {
                var savedScript = GetSqlScript(id);
                result.SavedSqlScript = savedScript;
                return result;
            }
             
        }

        ///<inheritdoc/>
        public void EditQuery(long id, PagedQueryModel model)
        {
            var query = Context.Queries.FirstOrDefault(x => x.Id == id);
            if (query == null)
            {
                throw new BadRequestException("Query doen1t exist.");
            }

            query.QueryText = JsonConvert.SerializeObject(model);
            Context.SaveChanges();
        }

        public long EditQuerySqlScript(long id, string sqlScript)
        {
            var savedQuery = Context.Queries.FirstOrDefault(x => x.Id == id);
            var query = JsonConvert.DeserializeObject<PagedQueryModel>(savedQuery.QueryText);
            if (savedQuery == null)
            {
                throw new BadRequestException("Query doesn't exist");
            }
            var saveScriptModel = new SqlScriptSaveModel
            {
                Name = savedQuery.Name,
                CategoryId = savedQuery.CategoryId,
                SqlRequest = new SqlRequestModel
                {
                    ConnectionStringName = query?.ConnectionStringName,
                    PaginationModel = query?.PagingModel,
                    SqlScript = sqlScript
                }
            };
            var newId = AddSqlScript(saveScriptModel);
            DeleteQuery(id);
            return newId;
        }
        ///<inheritdoc/>
        public List<QueryViewModel> GetQueries()
        {
            return this.Context.Queries
                               .Select(x => new QueryViewModel
                               {
                                   Id = x.Id,
                                   Name = x.Name
                               }).ToList();
        }

        ///<inheritdoc/>
        public SavedQueryModel GetQuery(long id)
        {
            var savedQuery = GetQueryById(id);
            var query = JsonConvert.DeserializeObject<PagedQueryModel>(savedQuery.QueryText);
            return new SavedQueryModel
            {
                Filters = query.Queries.SelectMany(x=>x.GetExplicitFilters()).Distinct().ToList(),
                Columns = query.Queries[0].GetColumnsList()
            };
        }

        ///<inheritdoc/>
        public SavedQueryResult ProcessSavedQuery(long id, List<ExplicitFilterModel> filters)
        {
            var savedQuery = GetQueryById(id);
            var query = JsonConvert.DeserializeObject<PagedQueryModel>(savedQuery.QueryText);

            var res = new SavedQueryResult()
            {
                Id = savedQuery.Id,
                Name = savedQuery.Name,
                Query = query
            };
            foreach (var filter in filters)
            {
                if (filter.ColumnType == ColumnTypes.Date && filter.Value.Length == 4)
                {
                    filter.ColumnName = $"year({filter.ColumnName})";
                }

                foreach (var quer in res.Query.Queries.Where(x=>x.TableQueries.Any(y=>y.TableName==filter.TableName)))
                {
                    quer.Filters.Add(new WhereGroup()
                    {
                        WhereStatements = new List<WhereModel>{new WhereModel
                        {
                            ColumnAlias = filter.ColumnName,
                            TableId = quer.TableQueries.FirstOrDefault(x=>x.TableName==filter.TableName).Id,
                            Comparison = filter.ComparisonType,
                            Value = filter.Value
                        }},
                        WhereGroups = new List<WhereGroup>(),
                        IsExternal = true
                    });
                }
               
            }
            return res;
        }

        ///<inheritdoc/>
        public void DeleteQuery(long id)
        {
            var savedQuery = GetQueryById(id);
            this.Context.Queries.Remove(savedQuery);
            this.Context.SaveChanges();
        }

        #endregion

        #region SqlScript region
        public long AddSqlScript(SqlScriptSaveModel sqlScript)
        {
            var model = new SqlScript
            {
                CategoryId = sqlScript.CategoryId,
                Name = sqlScript.Name,
                SqlScriptText = JsonConvert.SerializeObject(sqlScript.SqlRequest),
                Parameters =JsonConvert.SerializeObject(sqlScript.Parameters)
            };
            Context.SqlScripts.Add(model);
            Context.SaveChanges();
            return model.Id;
        }

        public List<QueryViewModel> GetSqlScripts()
        {
            var scripts = Context.SqlScripts.Select(script => new QueryViewModel
            {
                Id = script.Id,
                Name = script.Name
            }).ToList();
            return scripts;
        }

        public SavedScriptModel GetSqlScript(long id)
        {
            var script = Context.SqlScripts.FirstOrDefault(x => x.Id == id);
            if (script == null) return new SavedScriptModel();
            var scriptModel = JsonConvert.DeserializeObject<SqlRequestModel>(script.SqlScriptText);
            var parameters = JsonConvert.DeserializeObject<List<string>>(script.Parameters);
            return new SavedScriptModel
            {
                SqlScript = scriptModel.SqlScript,
                Parameters = parameters,
                ConnectionStringName = scriptModel.ConnectionStringName
            };
        }

        public SqlRequestModel ProcessSavedSqlScript(long id)
        {
            var scriptModel = Context.SqlScripts.FirstOrDefault(x => x.Id == id);
            if(scriptModel == null)
            {
                throw new BadRequestException("Query doen1t exist.");
            }
            return JsonConvert.DeserializeObject<SqlRequestModel>(scriptModel.SqlScriptText);
        }

        public void DeleteSqlScript(long id)
        {
            var script = Context.SqlScripts.FirstOrDefault(x => x.Id == id);
            if (script != null)
            {
                Context.SqlScripts.Remove(script);
                Context.SaveChanges();
            }
            
        }

        public void EditSqlScript(long id, SqlRequestModel model)
        {
            var script = Context.SqlScripts.FirstOrDefault(x => x.Id == id);
            if (script == null)
            {
                throw new BadRequestException("Query doen1t exist.");
            }
            script.SqlScriptText = JsonConvert.SerializeObject(model);
            Context.SaveChanges();

        }
        #endregion

        #region Helpers

        private Query GetQueryById(long id)
        {
            var query = Context.Queries.FirstOrDefault(x => x.Id == id);
            if (query == null)
            {
                throw new BadRequestException("Query with specified id is not found.");
            }
            return query;
        }

        private WhereGroup GetWhereGroup(ExplicitFilterModel filter)
        {
            return new WhereGroup
            {
                WhereStatements = new List<WhereModel>
                {
                    new WhereModel
                    {
                        ColumnAlias = filter.ColumnName,
                        Value = filter.Value,
                        Comparison = filter.ComparisonType
                    }
                },
                WhereGroups = new List<WhereGroup>()
            };
        }

      
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Devabit.Telelingua.ReportingServices.DataManagers.Interface;
using Devabit.Telelingua.ReportingServices.DAL.Interface;
using Devabit.Telelingua.ReportingServices.Helpers;
using Devabit.Telelingua.ReportingServices.Models.DataModels;

namespace Devabit.Telelingua.ReportingServices.DataManagers.Implementation
{
    public class TableDataManager : ITableDataManager
    {
        private readonly ITableDal dal;

        public TableDataManager(ITableDal dal)
        {
            this.dal = dal;
        }

        ///<inheritdoc/>
        public IEnumerable<TableViewModel> GetTableNames()
        {
            return dal.GetTableNames();
        }

        ///<inheritdoc/>
        public TableDataModel GetTable(TableViewModel table)
        {
            if (dal.GetTableNames().Count(x => (string.Equals(x.Name, table.Name, StringComparison.Ordinal)
                                                && string.Equals(x.Schema, table.Schema, StringComparison.Ordinal))) == 0)
            {
                throw new BadRequestException("Table with specified name and schema doesn't exist.");

            }
            return this.dal.GetTable(table);
        }

        ///<inheritdoc/>
        public PagedQueryResultModel ProcessQuery(PagedQueryModel queries, List<ComparableFieldModel> comparisons = null, List<CalculatedColumnModel> externalCalculation = null)
        {
            //this.ValidateSelectQueries(queries.Query);
            //this.ValidateJoinQueries(queries.Query);
            var paginated = InjectPaginations(queries);
            return this.GetDataset(paginated.ToList(), queries.PagingModel.PageSize, queries.PagingModel.PageNumber);
        }
        //TODO: implement method
        public PagedQueryResultModel ProcessSqlScript(SqlScriptModel sqlScriptModel)
        {
            return dal.ProcessSqlScript(sqlScriptModel);
        }

        private IEnumerable<PagedQueryModel> InjectPaginations(PagedQueryModel queries)
        {
            var mustTake = queries.PagingModel.PageSize;
            var firstToTake = (queries.PagingModel.PageNumber - 1) * queries.PagingModel.PageSize;
            foreach (var query in queries.Queries)
            {
                var count = this.dal.GetRecordsCount(query, queries.ConnectionStringName);
                if (firstToTake > count)
                {
                    yield return new PagedQueryModel()
                    {
                        PagingModel = new PaginationModel()
                        {
                            PageSize = 0,
                            PageNumber = 0
                        },
                        Queries = new List<QueryModel>() { query }
                    };
                    firstToTake -= count;
                }
                else
                {
                    if (mustTake + firstToTake < count)
                    {
                        yield return new PagedQueryModel()
                        {
                            PagingModel = new PaginationModel()
                            {
                                Skip = firstToTake,
                                PageSize = mustTake,
                                PageNumber = 1
                            },
                            Queries = new List<QueryModel>() { query }
                        };
                    }
                    else
                    {
                        yield return new PagedQueryModel()
                        {
                            PagingModel = new PaginationModel()
                            {
                                Skip = firstToTake,
                                PageSize = mustTake,
                                PageNumber = 1
                            },
                            Queries = new List<QueryModel>() { query }
                        };
                        mustTake -= (count - firstToTake);
                        firstToTake = 0;
                    }
                }
            }
        }

        private PagedQueryResultModel GetDataset(List<PagedQueryModel> queries, long pageSize, long PageNumber, List<ComparableFieldModel> comparisons = null, List<CalculatedColumnModel> externalCalculation = null)
        {
            var results = queries.Select(x => this.dal.ProcessQuery(x,x.ConnectionStringName,comparisons,externalCalculation)).ToList();
            var first = results.FirstOrDefault(x => x.Result.Rows.Any());
            if(first == null)
            {
                return results.First();
            }
            results.Remove(first);
            var baseIndex = first.Result.Rows.Count;
            foreach (var res in results)
            {
                first.TotalCount += res.TotalCount;
                if (!res.Result.Rows.Any())
                {
                    continue;
                }
                for (int i = 0; i < res.Result.Rows.Count; i++)
                {
                    var row = new RowModel
                    {
                        Values = new List<string>()
                    };
                    for (int j = 0; j < res.Result.ColumnHeaders.Count; j++)
                    {
                        row.Values.Add("");
                    }
                    first.Result.Rows.Add(row);
                }
                var headerindex = 0;
                foreach (var header in res.Result.ColumnHeaders)
                {
                    var index = first.Result.ColumnHeaders.FindIndex(x => x.Name == header.Name);
                    var rowIndex = 0;
                    foreach (var row in res.Result.Rows)
                    {
                        first.Result.Rows.ElementAt(baseIndex + rowIndex).Values[index] = row.Values[headerindex];
                        rowIndex++;
                    }

                    headerindex++;
                }

                var indexRow = 0;
                foreach (var row in res.Result.Rows)
                {
                    first.Result.Rows[baseIndex + indexRow].Internal = row.Internal;
                    indexRow++;
                }

                baseIndex += res.Result.Rows.Count;
            }

            first.Result.Rows = first.Result.Rows.Take((int)pageSize).ToList();
            first.PageCount = first.TotalCount / pageSize + 1;
            first.PageNumber = PageNumber;
            first.HasNext = PageNumber < first.PageCount;
            return first;
        }

        ///<inheritdoc/>
        private void ValidateSelectQueries(QueryModel queries)
        {
            foreach (var query in queries.TableQueries)
            {
                var table = this.GetTable(new TableViewModel()
                {
                    Name = query.TableName,
                    Schema = query.TableSchema
                }).Columns.Select(x => x.Key);

                foreach (var column in query.SelectedColumns)
                {
                    if (!table.Contains(column.ColumnName))
                    {
                        throw new BadRequestException($"The specified column {column} is not present in db.");
                    }
                }
            }
        }

        ///<inheritdoc/>
        private void ValidateJoinQueries(QueryModel queries)
        {
            if (queries.TableQueries.Count > 1 && queries.TableQueries.All(x => x.Joins.Count == 0))
            {
                throw new BadRequestException("Can`t select from two tables without joining them.");
            }

            foreach (var query in queries.TableQueries)
            {
                var leftColumns = this.GetTable(new TableViewModel()
                {
                    Name = query.TableName,
                    Schema = query.TableSchema
                }).Columns;

                foreach (var join in query.Joins)
                {
                    var rightColumns = this.GetTable(join.ToTable).Columns;

                    //if (leftColumns.All(x => x.Key != join.FromColumn))
                    //{
                    //    throw new BadRequestException($"Can`t join on column {join.FromColumn}");
                    //}

                    //if (rightColumns.All(x => x.Key != join.OnColumn))
                    //{
                    //    throw new BadRequestException($"Can`t join on column {join.OnColumn}");
                    //}

                    //if (leftColumns[join.FromColumn] != rightColumns[join.OnColumn])
                    //{
                    //    throw new BadRequestException("Cant join : different column types");
                    //}
                }
            }
        }

       
    }
}

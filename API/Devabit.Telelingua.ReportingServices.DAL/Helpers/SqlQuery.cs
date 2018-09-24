using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Devabit.Telelingua.ReportingServices.DAL.Helpers
{
    public class SqlQuery
    {
        #region Fields
        private string _distinctString = string.Empty;
        private string _columnsString = string.Empty;
        private string _functionString = string.Empty;
        private string _tablesString = string.Empty;
        private string _joinsString = string.Empty;
        private string _whereString = string.Empty;
        private string _groupByString = string.Empty;
        private string _orderByString = string.Empty;
        private string _offsetString = string.Empty;
        private string _fetchString = string.Empty;

        // private IEnumerable<string> _groupByColumns;
        private IEnumerable<string> _selectedColumns;
        private IEnumerable<string> _functions;
        #endregion

        #region Properties
        public bool HasGroupBy => GroupByColumns != null;

        public List<string> GroupByColumns { get; private set; }
        #endregion

        #region Methods
        public SqlQuery AddDistinct()
        {
            _distinctString = "distinct ";
            return this;
        }

        public SqlQuery AddSelectedColumns(List<string> columns)
        {
            _selectedColumns = columns;
            SetColumnsAlias(columns);
            _columnsString = string.Join(",", columns);
            return this;
        }

        public SqlQuery AddFunctions(List<string> functions)
        {
            _functions = functions;
            if (functions.Count != 0)
            {
                _functionString = string.Join(",", functions);
            }
            if (!string.IsNullOrEmpty(_columnsString) && !string.IsNullOrEmpty(_functionString))
            {
                _functionString = $", {_functionString}";
            }
            return this;
        }

        public SqlQuery AddSelectionTable(string table)
        {
            _tablesString = $"from {table} ";
            return this;
        }

        public SqlQuery AddJoins(List<string> joins)
        {
            if (joins.Count != 0)
            {
                _joinsString = string.Join(Environment.NewLine, joins);
            }
            return this;
        }

        public SqlQuery AddWheres(List<string> wheres)
        {
            if (wheres.Count != 0)
            {
                _whereString = $" where {string.Join(" ", wheres)}";
            }
            return this;
        }

        public SqlQuery AddGroupBys(List<string> groupBys)
        {
            if (groupBys.Count != 0)
            {
                GroupByColumns = groupBys;
                _groupByString = $" group by {string.Join(",", groupBys)}";
            }
            return this;
        }

        public SqlQuery AddOrderBys(List<string> orderBys)
        {
            if (orderBys.Count != 0)
            {
                _orderByString = $" order by {string.Join(",", orderBys)}";
            }
            return this;
        }

        public SqlQuery AddOffset(long offset)
        {
            _offsetString = $" OFFSET {offset} ROWS";
            return this;
        }

        public SqlQuery AddFetch(long count)
        {
            _fetchString = $" FETCH NEXT {count} ROWS ONLY";
            return this;
        }

        public void ChangeSelectedColumnsIfGroupBy(bool withGroupDetails = false)
        {
            if (GroupByColumns != null)
            {
                var groupByColumnsName = new List<string>(); // = GroupByColumns.Select(name => _selectedColumns.FirstOrDefault(x => x.Contains(name)));
                foreach (var groupColumn  in GroupByColumns)
                {
                    var columnName = _selectedColumns.FirstOrDefault(x => x.Contains(groupColumn)) ?? (_functions.FirstOrDefault(f => f.Contains(groupColumn))!=null?groupColumn:null);
                    if (columnName != null)
                    {
                        groupByColumnsName.Add(columnName);
                    }
                }

                var groupByColumnsString = string.Join(",", groupByColumnsName);
                var selectedColumnsString = string.Join(",", _selectedColumns) ;
                
                _columnsString = withGroupDetails && !string.IsNullOrEmpty(selectedColumnsString) ? string.Join(",", groupByColumnsString, selectedColumnsString) : groupByColumnsString;
                if (withGroupDetails)
                {
                    _groupByString = string.Empty;
                    _offsetString = string.Empty;
                    _fetchString = string.Empty;
                    _functionString = string.Empty;
                }

            }
        }

        public override string ToString()
        {
            return $"select {_distinctString} {_columnsString} {_functionString} {_tablesString} " +
                   $"{_joinsString} {_whereString} {_groupByString} {_orderByString} {_offsetString} {_fetchString}";
        }

        #endregion

        #region Helpers
        private void SetColumnsAlias(List<string> columns)
        {
            var count = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                var columnName = columns[i].Split('.')[1];
                var tableName = columns[i].Split('.')[0];
                if (columns.FindAll(c => c.Substring(c.IndexOf('.') + 1) == columnName).Count > 1)
                {
                    var regex = new Regex(@"\w+\s+as\s+\w+");
                    if (regex.IsMatch(columns[i]))
                    {
                        columns[i] = $"{tableName}.{columnName}{ count++}";
                    }
                    else
                    {
                        columns[i] = $"{columns[i]} as {columnName}{count++}";
                    }

                }
            }
        }
        #endregion
    }
}

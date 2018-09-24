using System;
using System.Collections.Generic;
using System.Text;
using Devabit.Telelingua.ReportingServices.DataManagers.Implementation;
using Devabit.Telelingua.ReportingServices.DataManagers.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Devabit.Telelingua.ReportingServices.DataManagers
{
    public static class DataManagerDependencies
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped<ITableDataManager, TableDataManager>();
            services.AddScoped<IQueryDataManager, QueryDataManager>();
        }
    }
}

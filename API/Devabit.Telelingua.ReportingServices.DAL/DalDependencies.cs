using System;
using System.Collections.Generic;
using System.Text;
using Devabit.Telelingua.ReportingServices.DAL.Implementation;
using Devabit.Telelingua.ReportingServices.DAL.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Devabit.Telelingua.ReportingServices.DAL
{
    public static class DalDependencies
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped<ITableDal, TableDal>();
            services.AddScoped<IQueryDal, QueryDal>();
        }
    }
}

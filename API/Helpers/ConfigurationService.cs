using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Devabit.Telelingua.ReportingServices.Helpers
{
    public class ConfigurationService
    {
        private IConfiguration configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string ConnectionString => configuration.GetConnectionString("DbConnection");

        public List<string> GetUserConnections()
        {
            return configuration.GetSection("UserConnectionStrings").GetChildren().Select(x => x.Key).ToList();
        }

        public string GetUserConnectionString(string name)
        {
            return configuration.GetSection("UserConnectionStrings")[name];
        }

        public string Secret => configuration.GetSection("Secret").Value;
    }
}

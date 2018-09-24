using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devabit.Telelingua.ReportingServices.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Devabit.Telelingua.ReportingServices.Web.Controllers.api
{
    [Authorize(Roles = "Administrator")]
    [Route("api/[controller]")]
    public class ConnectionsController: Controller
    {

        private readonly ConfigurationService config;

        public ConnectionsController(ConfigurationService config)
        {
            this.config = config;
        }

        [HttpGet]
        public List<string> Get()
        {
            return this.config.GetUserConnections();
        }
    }
}

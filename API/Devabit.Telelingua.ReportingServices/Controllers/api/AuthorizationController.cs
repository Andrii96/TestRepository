using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devabit.Telelingua.ReportingServices.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Flurl.Http;
using Devabit.Telelingua.ReportingServices.Models.TelelinguaModels;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Devabit.Telelingua.ReportingServices.Helpers;
using Devabit.Telelingua.ReportingServices.Models.ErrorModels;
using Devabit.Telelingua.ReportingServices.Models.Helpers;

namespace Devabit.Telelingua.ReportingServices.Web.Controllers.api
{
    [Produces("application/json")]
    [Route("api/Authorization")]
    public class AuthorizationController : Controller
    {
        private readonly string telelinguaAuth = "http://10.0.1.235:8076/api/TiltUsers/Authenticate";

        private readonly ConfigurationService config;

        public AuthorizationController(ConfigurationService config)
        {
            this.config = config;
        }
        
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Authorize(AuthorizationModel model)
        {
            try
            {
                var telelinguaResult = await telelinguaAuth.PostJsonAsync(model).ReceiveJson<TelelinguaAuthResponceModel>();
                if(telelinguaResult.User == null)
                {
                    return BadRequest("User name or password is invalid.");
                }
                var claims = new[]
                {
                new Claim("role",telelinguaResult.User.Role.Name.Replace(" ","")),
                new Claim("Entity",telelinguaResult.User.Entity.EntityName)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Secret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "reporting",
                    audience: "reporting",
                    claims: claims,
                    expires: DateTime.Now.AddHours(12),
                    signingCredentials: creds);

                return Ok(new TokenViewModel
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Role = claims[0].Value,
                    Entity = claims[1].Value
                });
            }catch(Exception e)
            {
                return StatusCode(500,new ErrorViewModel { ErrorCode = 500, ErrorDescription = e.Message });
            }
            
        }
    }
}
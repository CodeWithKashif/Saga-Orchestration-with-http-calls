using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.AspNetCore.Http;
using CustomerManagementAPI.Models;
using Microsoft.Extensions.Configuration;
using Dapper;
using Microsoft.AspNetCore.Hosting;

namespace CustomerManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {

        private readonly ILogger<CustomerController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;

        public CustomerController(ILogger<CustomerController> logger, 
            IConfiguration iConfig, 
            IHostingEnvironment env)
        {
            _logger = logger;
            _configuration = iConfig;
            _env = env;
        }

        [HttpPost]
        [Route(ApiEndPoint.Register)]
        public async Task<IActionResult> RegisterAsync([FromBody] Customer customer)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();

                using (IDbConnection dbConnection = new SqlConnection(GetConnectionString()))
                {
                    string sql = @"Insert into Customer(EmailAddress, Name, TelephoneNumber) 
                                    values(@EmailAddress, @Name, @TelephoneNumber) ";

                    int rowsAffected = await dbConnection.ExecuteAsync(sql, customer);
                }

                return Ok(new {customer.EmailAddress});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            //TODO: add for conflicts
        }
        
        [HttpGet]
        [Route(ApiEndPoint.UndoRegister)]
        public async Task<IActionResult> UndoRegisterAsync(string emailAddress)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                
                //// Undo insert customer
                using (IDbConnection dbConnection = new SqlConnection(GetConnectionString()))
                {
                    string sql = "DELETE FROM Customer WHERE EmailAddress = @emailAddress ";
                    int rowsAffected = await dbConnection.ExecuteAsync(sql, new {emailAddress});
                }

                return Ok(new {emailAddress});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        [Route(ApiEndPoint.SendWelcomeEmail)]
        public IActionResult SendWelcomeEmail(string emailAddress)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();

                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Welcome Email failed");
                _logger.LogError(ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private string GetConnectionString()
        {
            string connectionString = _configuration.GetConnectionString("CustomerManagementCN");
            if(connectionString.Contains("%CONTENTROOTPATH%"))
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", _env.ContentRootPath);

            return connectionString;
        }

    }
}

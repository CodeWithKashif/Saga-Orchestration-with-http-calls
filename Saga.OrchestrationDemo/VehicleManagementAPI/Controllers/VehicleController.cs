using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using VehicleManagementAPI.Models;

namespace VehicleManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehicleController : ControllerBase
    {

        private readonly ILogger<VehicleController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _env;
        
        public VehicleController(ILogger<VehicleController> logger, 
            IConfiguration iConfig, IHostingEnvironment env)
        {
            _logger = logger;
            _configuration = iConfig;
            _env = env;
        }

        [HttpPost]
        [Route(ApiEndPoint.Register)]
        public async Task<IActionResult> RegisterAsync([FromBody] Vehicle vehicle)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                
                if(vehicle.GenerateDemoError)
                    throw new InvalidOperationException("Generated Demo Error based on the input");

                using (IDbConnection dbConnection = new SqlConnection(GetConnectionString()))
                {
                    string sql = @"Insert into Vehicle(LicenseNumber, Brand, Type, OwnerId) 
                                    values(@LicenseNumber, @Brand, @Type, @OwnerId) ";

                    int rowsAffected = await dbConnection.ExecuteAsync(sql, vehicle);
                }

                return Ok(new { vehicle.LicenseNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        [HttpGet]
        [Route(ApiEndPoint.UndoRegister)]
        public async Task<IActionResult> UndoRegisterAsync(string licenseNumber)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest();
                
                //// Undo insert Vehicle
                using (IDbConnection dbConnection =
                    new SqlConnection(GetConnectionString()))
                {
                    string sql = "DELETE FROM Vehicle WHERE LicenseNumber = @licenseNumber ";
                    int rowsAffected = await dbConnection.ExecuteAsync(sql, new {licenseNumber});
                }

                return Ok(new {licenseNumber});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _logger.LogError(ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private string GetConnectionString()
        {
            string connectionString = _configuration.GetConnectionString("VehicleManagementCN");
            if(connectionString.Contains("%CONTENTROOTPATH%"))
                connectionString = connectionString.Replace("%CONTENTROOTPATH%", _env.ContentRootPath);

            return connectionString;
        }

    }
}

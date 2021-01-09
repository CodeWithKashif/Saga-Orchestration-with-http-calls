using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using CustomerManagementAPI.Models;

namespace CustomerManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {


            //var con = ConfigurationManager.ConnectionStrings["dbconnection"].ToString();


            using (var myConnection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=E:\\MyDevStore\\DDD\\SAGA\\Saga.OrchestrationDemo\\CustomerManagementAPI\\App_Data\\CustomerManagementDB.mdf;Integrated Security=True"))
            {
                myConnection.Open();
                Console.WriteLine(myConnection.ConnectionString);
                Console.WriteLine(myConnection.State);

                SqlCommand aa = new SqlCommand("Insert into Customer(Id, Name) values(2,'aa') ", myConnection);
                //int aaa = aa.ExecuteNonQuery();
                myConnection.Close();
                Console.WriteLine(myConnection.State);
            }

            return "CustomerManagementAPI";
        }
    }
}

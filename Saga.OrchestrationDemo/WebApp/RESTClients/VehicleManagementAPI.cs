using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.RESTClients
{
    public class VehicleManagementAPI : RESTClientsBase, IVehicleManagementAPI
    {
        private readonly ILogger<VehicleManagementAPI> _logger;

        public VehicleManagementAPI(IConfiguration config, ILogger<VehicleManagementAPI> logger) 
            : base(config, "VehicleManagementAPI")
        {
            _logger = logger;
        }

        public async Task<HttpResponseMessage> Register(VehicleRegisterVM vehicleRegisterVM, string emailAddress)
        {
            _logger.LogInformation($"SAGA - vehicleRegisterVM : {JsonConvert.SerializeObject(vehicleRegisterVM)}" );
            _logger.LogInformation($"SAGA - emailAddress : {emailAddress}" );

            var vehicle = Mapper.Map<Vehicle>(vehicleRegisterVM);
            vehicle.OwnerId = emailAddress;
            HttpResponseMessage httpResponse =  await Post(vehicle, "vehicle/register");
            _logger.LogInformation($"SAGA - httpResponse : {JsonConvert.SerializeObject(httpResponse)}" );
            string licenseNumber = httpResponse.Content.ReadAsStringAsync().Result;
            return httpResponse;
        }

        public async Task<HttpResponseMessage> UndoRegister(string licenseNumber)
        {
            _logger.LogInformation($"SAGA - VehicleManagementAPI.UndoRegister -licenseNumber : {licenseNumber}" );

            HttpResponseMessage httpResponse = await Get("vehicle/UndoRegister/", licenseNumber);
            _logger.LogInformation($"SAGA - httpResponse : {JsonConvert.SerializeObject(httpResponse)}");
            return httpResponse;
        }
    }
}
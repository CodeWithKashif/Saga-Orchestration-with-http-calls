using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.RESTClients
{
    public class WorkshopManagementAPI : RESTClientsBase, IWorkshopManagementAPI
    {
        private readonly ILogger<WorkshopManagementAPI> _logger;

        public WorkshopManagementAPI(IConfiguration config, ILogger<WorkshopManagementAPI> logger) 
            : base(config, "WorkshopManagementAPI")
        {
            _logger = logger;
        }

        public async Task<HttpResponseMessage> RegisterPlanning(PlanMaintenanceJobVM maintenanceJobVM, 
                                        string emailAddress, string licenseNumber)
        {
            _logger.LogInformation($"SAGA - maintenanceJobVM : {JsonConvert.SerializeObject(maintenanceJobVM)}" );
            _logger.LogInformation($"SAGA - emailAddress : {emailAddress} - licenseNumber : {licenseNumber}" );

            var maintenanceJob = Mapper.Map<MaintenanceJob>(maintenanceJobVM);
            maintenanceJob.JobId = Guid.NewGuid();
            _logger.LogInformation($"SAGA - maintenanceJob.JobId: {maintenanceJob.JobId}" );

            maintenanceJob.OwnerId = emailAddress;
            maintenanceJob.LicenseNumber = licenseNumber;
            maintenanceJob.PlanningDate = DateTime.Now;//TODO:Need to revisit later
            
            HttpResponseMessage httpResponse = await Post(maintenanceJob, "WorkshopPlanning/PlanMaintenanceJob");
            _logger.LogInformation($"SAGA - httpResponse : {JsonConvert.SerializeObject(httpResponse)}" );
            return httpResponse;
        }

        public async Task<HttpResponseMessage> SendMaintenanceJobScheduleDetailEmail(string emailAddress)
        {
            HttpResponseMessage httpResponse= await Get("WorkshopPlanning/SendMaintenanceJobScheduleDetailEmail/", emailAddress);
            _logger.LogInformation($"Maintenance Job Schedule Detail Email sent to {emailAddress}");

            return httpResponse;
        }
    }
}

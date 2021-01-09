using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Refit;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.RESTClients
{
    public interface IWorkshopManagementAPI
    {
        Task<HttpResponseMessage> RegisterPlanning(PlanMaintenanceJobVM maintenanceJobVM, string emailAddress,
                                                    string licenseNumber);

        Task<HttpResponseMessage> SendMaintenanceJobScheduleDetailEmail(string emailAddress);
    }
}

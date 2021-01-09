using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using WebApp.RESTClients;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class WorkshopManagementController : Controller
    {
        private readonly ILogger<WorkshopManagementController> _logger;
        private readonly ICustomerManagementAPI _customerAPI;
        private readonly IVehicleManagementAPI _vehicleAPI;
        private readonly IWorkshopManagementAPI _workshopAPI;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _policy;
        public WorkshopManagementController(ILogger<WorkshopManagementController> logger, 
                ICustomerManagementAPI customerAPI, 
                IVehicleManagementAPI vehicleAPI, 
                IWorkshopManagementAPI workshopAPI)
        {
            _logger = logger;
            _customerAPI = customerAPI;
            _vehicleAPI = vehicleAPI;
            _workshopAPI = workshopAPI;
            this._policy = GetRetryPolicy();;
        }
        public IActionResult Index()
        {
            return View("~/Views/Home/Index.cshtml");
        }

        [HttpGet]
        public IActionResult New()
        {
            var model = new WorkShopManagementNewVM
            {
                Customer = new CustomerRegisterVM()
            };
            return View(model);
        }

        /// <summary>
        /// SAGA Orchestrator
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterAndPlanMaintenanceJob([FromForm] WorkShopManagementNewVM inputModel)
        {
            if (ModelState.IsValid)
            {
                string emailAddress = inputModel.Customer.EmailAddress;
                string licenseNumber = inputModel.Vehicle.LicenseNumber;

                HttpResponseMessage customerResponse = await _customerAPI.Register(inputModel.Customer);
                if (ShowErrorIfRequired(customerResponse, out var customerError)) return customerError;

                HttpResponseMessage vehicleApiResponse = await _vehicleAPI.Register(inputModel.Vehicle, emailAddress);
                await CompensateIfRequired(inputModel, vehicleApiResponse);
                if (ShowErrorIfRequired(vehicleApiResponse, out var vehicleError)) return vehicleError;

                HttpResponseMessage workShopApiResponse = await _workshopAPI.RegisterPlanning(inputModel.MaintenanceJob, emailAddress, licenseNumber);
                await CompensateIfRequired(inputModel, workShopApiResponse);
                if (ShowErrorIfRequired(workShopApiResponse, out var workShopError)) return workShopError;

                await Notify(emailAddress);

                TempData["SuccessMessage"] = "Success! Please check your email to see the Maintenance Job Detail.";
                return RedirectToAction("Index");
            }
            else
            {
                return View("New", inputModel);
            }
        }

        private async Task Notify(string emailAddress)
        {
            /*If these emails fail we wont consider it as a transaction failure
            and that's the business decision you have to take. 
            Otherwise you have to send another email to disregard(compensate) the previous email*/
            await _customerAPI.SendWelcomeEmail(emailAddress);
            await _workshopAPI.SendMaintenanceJobScheduleDetailEmail(emailAddress);
        }

        /// <summary>
        /// Always Apply Retry pattern to make it more resilient 
        /// </summary>
        private async Task CompensateIfRequired(WorkShopManagementNewVM inputModel, HttpResponseMessage httpResponse)
        {
            _logger.LogInformation("SAGA - CompensateIfRequired called" );

            if (httpResponse.StatusCode == HttpStatusCode.OK) return;
            _logger.LogInformation($"SAGA - httpResponse.StatusCode{httpResponse.StatusCode}");

            if(IsVehicleApiRequest(httpResponse))
            {
                _logger.LogInformation("SAGA - IsVehicleApiRequest:true");
                HttpResponseMessage customerUndoResponse = await _policy
                    .ExecuteAsync(() => _customerAPI.UndoRegister(inputModel.Customer.EmailAddress));
            }
            else if (IsWorkshopApiRequest(httpResponse))
            {
                _logger.LogInformation("SAGA - IsWorkshopApiRequest:true");
                HttpResponseMessage vehicleUndoResponse =  await _policy
                    .ExecuteAsync(() => _vehicleAPI.UndoRegister(inputModel.Vehicle.LicenseNumber));

                HttpResponseMessage customerUndoResponse = await _policy
                    .ExecuteAsync(() => _customerAPI.UndoRegister(inputModel.Customer.EmailAddress));
            }
        }
      
        private bool ShowErrorIfRequired(HttpResponseMessage response, out IActionResult actionResult)
        {
            _logger.LogInformation("SAGA - httpResponse : ShowErrorIfRequired called." );

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogError($"SAGA - response.StatusCode:{response.StatusCode}" );
                ModelState.Clear();
                if (IsCustomerApiRequest(response))
                {
                    ModelState.AddModelError("Email", "Email Already Exist");
                    {
                        actionResult = View("New");
                        return true;
                    }
                }
                if(IsVehicleApiRequest(response))
                {
                    ModelState.Clear();
                    ModelState.AddModelError("LicenseNumber", "LicenseNumber Already Exist");
                    actionResult = View("New");
                    return true;
                }
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                ModelState.Clear();
                ModelState.AddModelError("Error", "Something went wrong and your transaction is canceled");
                actionResult = View("New");
                return true;
            }

            actionResult = null;
            return false;
        }

        private static bool IsCustomerApiRequest(HttpResponseMessage response)
        {
            return response.RequestMessage.RequestUri.AbsolutePath.ToLower().Contains("customer/register");
        }

        private static bool IsVehicleApiRequest(HttpResponseMessage response)
        {
            return response.RequestMessage.RequestUri.AbsolutePath.ToLower().Contains("vehicle/register");
        }

        private static bool IsWorkshopApiRequest(HttpResponseMessage response)
        {
            return response.RequestMessage.RequestUri.AbsolutePath.ToLower().Contains("workshopplanning/planmaintenancejob");
        }

        private AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy.Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3, (ex, retryCount) =>
                {
                    _logger.LogInformation($"Retry count {retryCount}");
                });
        }

    }

}

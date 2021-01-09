using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using WebApp.Controllers;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.RESTClients
{
    public class CustomerManagementAPI : RESTClientsBase, ICustomerManagementAPI
    {
        private readonly ILogger<CustomerManagementAPI> _logger;

        public CustomerManagementAPI(IConfiguration config, ILogger<CustomerManagementAPI> logger)
            : base(config, "CustomerManagementAPI")
        {
            _logger = logger;
        }

        public async Task<HttpResponseMessage> Register(CustomerRegisterVM customerRegisterVM)
        {
            _logger.LogInformation($"SAGA - customerRegisterVM : {JsonConvert.SerializeObject(customerRegisterVM)}" );
            var customer = Mapper.Map<Customer>(customerRegisterVM);
            
            HttpResponseMessage httpResponse = await Post(customer, "customer/register");
            _logger.LogInformation($"SAGA - httpResponse : {JsonConvert.SerializeObject(httpResponse)}" );
            string emailAddress = httpResponse.Content.ReadAsStringAsync().Result;
            return httpResponse;
        }

        public async Task<HttpResponseMessage> UndoRegister(string emailAddress)
        {
            _logger.LogInformation($"SAGA - CustomerManagementAPI.UndoRegister -emailAddress : {emailAddress}" );
            
            HttpResponseMessage httpResponse = await Get("customer/UndoRegister/", emailAddress);
            _logger.LogInformation($"SAGA - httpResponse : {JsonConvert.SerializeObject(httpResponse)}");
            return httpResponse;
        }

        public async Task<HttpResponseMessage> SendWelcomeEmail(string emailAddress)
        {
            HttpResponseMessage httpResponse = await Get("customer/SendWelcomeEmail/", emailAddress);
            _logger.LogInformation($"Welcome Email sent to {emailAddress}");
            return httpResponse;
        }
    }
}

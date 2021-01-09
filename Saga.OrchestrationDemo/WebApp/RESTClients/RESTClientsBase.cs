using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WebApp.Models;

namespace WebApp.RESTClients
{
    public class RESTClientsBase
    {
        protected Uri HttpClientBaseAddress;

        public RESTClientsBase(IConfiguration config, string apiServiceName)
        {
            string apiHostAndPort = config.GetSection("APIServiceLocations")
                .GetValue<string>(apiServiceName);
            HttpClientBaseAddress = new Uri($"https://{apiHostAndPort}");
        }

        public async Task<HttpResponseMessage> Post(ModelBase model, string endPoint)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model),
                Encoding.UTF8, "application/json");

            return await new HttpClient()
                .PostAsync($"{HttpClientBaseAddress}{endPoint}", content);
        }

        public async Task<HttpResponseMessage> Get(string endPoint, string input)
        {
            var content = new StringContent(input, Encoding.UTF8, "application/json");
            return await new HttpClient().GetAsync($"{HttpClientBaseAddress}{endPoint}{input}");
        }

    }
}
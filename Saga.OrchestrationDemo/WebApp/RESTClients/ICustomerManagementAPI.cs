using System.Net.Http;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.RESTClients
{
    public interface ICustomerManagementAPI
    {
        Task<HttpResponseMessage> Register(CustomerRegisterVM customerVm);
        
        Task<HttpResponseMessage> UndoRegister(string emailAddress);
        
        Task<HttpResponseMessage> SendWelcomeEmail(string emailAddress);
    }
}

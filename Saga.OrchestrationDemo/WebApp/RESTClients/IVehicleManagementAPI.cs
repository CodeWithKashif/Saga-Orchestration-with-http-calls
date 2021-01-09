using System.Net.Http;
using System.Threading.Tasks;
using Refit;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.RESTClients
{
    public interface IVehicleManagementAPI
    {
        [Post("vehicle/register")]
        Task<HttpResponseMessage> Register(VehicleRegisterVM vehicleRegisterVM, string emailAddress);
        
        [Get("vehicle/UndoRegister/{licenseNumber}")]
        Task<HttpResponseMessage> UndoRegister(string licenseNumber);
    }
}

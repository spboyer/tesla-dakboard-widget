using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TeslaWidget.Models;

namespace TeslaWidget.Hubs
{
    public interface IStatus
    {
        Task SendStatus(Car[] cars);
    }


    public class StatusHub : Hub<IStatus>
    {
    }
}

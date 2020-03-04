using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Tesla.NET;
using Microsoft.Extensions.Options;
using TeslaWidget.Models;
using Newtonsoft.Json.Converters;

namespace TeslaWidget
{
    public interface ITeslaService
    {
        bool Authenticated();
        Task<double?> EstimatedRange();

        Task<Car[]> CarSummary();
    }

    public class TeslaService : ITeslaService
    {
        public HttpClient Client { get; }
        public Settings Settings { get; }

        private readonly TeslaAuthClient authClient;

        private readonly TeslaClient apiClient;

        public string Token { get; private set; }

        private List<Car> _cars;

        public TeslaService(HttpClient client, IOptions<Settings> options)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("tesla-net/0.6.2");
            Settings = options.Value;
            authClient = new TeslaAuthClient(client);
            apiClient = new TeslaClient(client);
            _cars = new List<Car>();

            // authenticate
            var authResult = authClient.RequestAccessTokenAsync(Settings.TESLA_CLIENT_ID, Settings.TESLA_CLIENT_SECRET, Settings.TESLA_LOGIN, Settings.TESLA_PWD).GetAwaiter().GetResult();
            Token = authResult.Data.AccessToken;
            
            Client = client;
        }


        public bool Authenticated()
        {
            return !String.IsNullOrEmpty(this.Token);
        }


        public async Task<Car[]> CarSummary()
        {
            var vehiclesResult = await apiClient.GetVehiclesAsync(Token);
            var vehicles = vehiclesResult.Data.Response;

            //vehicles.ForEach(v => { });
            foreach (var v in vehicles)
            {

                var item = _cars.FirstOrDefault(c => c.DisplayName == v.DisplayName);

                var car = new Car();

                if (item != null)
                {
                    car = item;
                }
                else 
                {
                    car.DisplayName = v.DisplayName;
                    car.Color = v.Color;
                    car.Id = v.Id;
                    car.VIN = v.Vin;
                    car.VehicleId = v.VehicleId;
                    _cars.Add(car);
                }

                var woke = false;
                var tries = 0;
                do
                {
                    tries++;
                    var wakeup = await apiClient.SendWakeUpAsync(v.Id, Token);
                    woke = (wakeup.HttpStatusCode == System.Net.HttpStatusCode.OK);

                } while (!woke | tries > 4);

                if (!woke)
                    break;

                try
                {
                    var state = await apiClient.GetVehicleStateAsync(v.Id, Token);
                    car.Odometer = state.Data.Response.Odometer;

                }
                catch
                {
                    car.Odometer = 0d;
                }
                
                var chargeResult = await apiClient.GetChargeStateAsync(v.Id, Token);
                var chargeState = chargeResult.Data.Response;


                car.EstBatteryRange = chargeState.EstBatteryRange;
                car.BatteryRange = chargeState.BatteryRange;
                car.BatteryLevel = chargeState.BatteryLevel;
                car.ChargingState = chargeState.ChargingState;
                car.SuperCharging = chargeState.FastChargerPresent;

               
            }

            return _cars.ToArray();
        }

        public async Task<double?> EstimatedRange()
        {
            // get vehicle list
            var vehiclesResult = await apiClient.GetVehiclesAsync(Token);
            var vehicles = vehiclesResult.Data.Response;

            // check charge status
            var chargeResult = apiClient.GetChargeStateAsync(vehicles.First().Id, Token).GetAwaiter().GetResult();
            var chargeState = chargeResult.Data.Response;

            return chargeState.EstBatteryRange;
        }
    }
}

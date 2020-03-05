using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeslaWidget.Models
{
    public class Car
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string VIN { get; set; }
        public string Color { get; set; }
        public double Odometer { get; set; }
        public string ChargingState { get; set; }
        public bool? SuperCharging { get; set; }
        public double? BatteryRange { get; set; }
        public double? EstBatteryRange { get; set; }
        public long? BatteryLevel { get; set; }
        public long VehicleId { get; internal set; }
        
        public string Image { 
            get
            {
                return ChargingState == "Charging" ? "Tesla_Model_3_charging" : "Tesla_Model_3";
            } 
        }
    }
}

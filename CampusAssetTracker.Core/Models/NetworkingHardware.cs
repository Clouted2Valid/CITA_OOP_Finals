using System;
using CampusAssetTracker.Core.Interfaces;

namespace CampusAssetTracker.Core.Models
{
    public class NetworkingHardware : CampusAsset, INetworkable
    {
        public string? IpAddress { get; private set; }
        public string? MacAddress { get; private set; }

        public NetworkingHardware(string assetTag, string name, decimal price, DateTime purchaseDate)
            : base(assetTag, name, price, purchaseDate) { }

        public override decimal CalculateDepreciation()
        {
            // 7-year lifecycle for networking gear
            var years = (DateTime.UtcNow - PurchaseDate).TotalDays / 365.0;
            var current = PurchasePrice - (PurchasePrice / 7m) * (decimal)years;
            return current < 0 ? 0 : current;
        }

        public override string GetMaintenanceStatus()
        {
            if (Status == AssetStatus.InRepair) return "In Repair";
            var age = (DateTime.UtcNow - PurchaseDate).TotalDays / 365.0;
            return age >= 7 ? "End of Life" : "Healthy";
        }

        public void AssignNetwork(string ipAddress, string macAddress)
        {
            IpAddress = ipAddress;
            MacAddress = macAddress;
        }
    }
}
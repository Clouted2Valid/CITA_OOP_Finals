using System;
using CampusAssetTracker.Core.Exceptions;
using CampusAssetTracker.Core.Interfaces;

namespace CampusAssetTracker.Core.Models
{
    public class Workstation : CampusAsset, IBorrowable, INetworkable
    {
        public string CpuSpec { get; set; } = string.Empty;

        public bool IsCheckedOut { get; private set; }
        public string? BorrowerId { get; private set; }

        public string? IpAddress { get; private set; }
        public string? MacAddress { get; private set; }

        public Workstation(string assetTag, string name, decimal price, DateTime purchaseDate, string cpuSpec)
            : base(assetTag, name, price, purchaseDate)
        {
            CpuSpec = cpuSpec;
        }

        // 5-year hardware depreciation, straight line
        public override decimal CalculateDepreciation()
        {
            var years = (DateTime.UtcNow - PurchaseDate).TotalDays / 365.0;
            var annualDepreciation = PurchasePrice / 5m;
            var total = annualDepreciation * (decimal)years;
            var current = PurchasePrice - total;
            return current < 0 ? 0 : current;
        }

        public override string GetMaintenanceStatus()
        {
            var age = (DateTime.UtcNow - PurchaseDate).TotalDays / 365.0;
            if (Status == AssetStatus.InRepair) return "In Repair";
            if (age >= 5) return "End of Life - Replace";
            if (age >= 4) return "Due for Replacement Soon";
            if (age >= 3) return "Schedule Inspection";
            return "Healthy";
        }

        public void CheckOut(string borrowerId)
        {
            if (Status == AssetStatus.InRepair)
                throw new AssetStateException("Cannot check out an asset that is in repair.");
            if (Status == AssetStatus.Retired)
                throw new AssetStateException("Cannot check out a retired asset.");
            if (IsCheckedOut)
                throw new AssetStateException("Asset is already checked out.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new AssetException("Borrower ID is required.");

            IsCheckedOut = true;
            BorrowerId = borrowerId;
            Status = AssetStatus.InUse;
        }

        public void CheckIn()
        {
            if (!IsCheckedOut)
                throw new AssetStateException("Asset is not checked out.");

            IsCheckedOut = false;
            BorrowerId = null;
            Status = AssetStatus.Available;
        }

        public void AssignNetwork(string ipAddress, string macAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(macAddress))
                throw new AssetException("IP and MAC are required.");
            IpAddress = ipAddress;
            MacAddress = macAddress;
        }
    }
}
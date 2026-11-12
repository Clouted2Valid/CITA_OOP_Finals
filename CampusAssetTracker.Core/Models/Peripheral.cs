using System;
using CampusAssetTracker.Core.Exceptions;
using CampusAssetTracker.Core.Interfaces;

namespace CampusAssetTracker.Core.Models
{
    public class Peripheral : CampusAsset, IBorrowable
    {
        public bool IsCheckedOut { get; private set; }
        public string? BorrowerId { get; private set; }

        public Peripheral(string assetTag, string name, decimal price, DateTime purchaseDate)
            : base(assetTag, name, price, purchaseDate) { }

        public override decimal CalculateDepreciation()
        {
            // 3-year lifecycle for peripherals
            var years = (DateTime.UtcNow - PurchaseDate).TotalDays / 365.0;
            var current = PurchasePrice - (PurchasePrice / 3m) * (decimal)years;
            return current < 0 ? 0 : current;
        }

        public override string GetMaintenanceStatus()
        {
            if (Status == AssetStatus.InRepair) return "In Repair";
            var age = (DateTime.UtcNow - PurchaseDate).TotalDays / 365.0;
            return age >= 3 ? "Replace" : "Healthy";
        }

        public void CheckOut(string borrowerId)
        {
            if (Status == AssetStatus.InRepair)
                throw new AssetStateException("Cannot check out an asset in repair.");
            if (IsCheckedOut)
                throw new AssetStateException("Peripheral already checked out.");
            IsCheckedOut = true;
            BorrowerId = borrowerId;
            Status = AssetStatus.InUse;
        }

        public void CheckIn()
        {
            if (!IsCheckedOut)
                throw new AssetStateException("Peripheral is not checked out.");
            IsCheckedOut = false;
            BorrowerId = null;
            Status = AssetStatus.Available;
        }
    }
}
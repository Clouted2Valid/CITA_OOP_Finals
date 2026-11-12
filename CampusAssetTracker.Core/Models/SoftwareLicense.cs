using System;
using CampusAssetTracker.Core.Exceptions;

namespace CampusAssetTracker.Core.Models
{
    public class SoftwareLicense : CampusAsset
    {
        public int SubscriptionMonths { get; set; }
        public DateTime SubscriptionEndDate { get; set; }

        public SoftwareLicense(string assetTag, string name, decimal price, DateTime purchaseDate,
            int subscriptionMonths, DateTime subscriptionEndDate)
            : base(assetTag, name, price, purchaseDate)
        {
            SubscriptionMonths = subscriptionMonths;
            SubscriptionEndDate = subscriptionEndDate;
        }

        public override decimal CalculateDepreciation()
        {
            // Depreciation based on subscription months remaining
            var monthsRemaining = (SubscriptionEndDate - DateTime.UtcNow).TotalDays / 30.0;
            if (monthsRemaining <= 0) return 0;
            var monthlyValue = PurchasePrice / SubscriptionMonths;
            var current = monthlyValue * (decimal)monthsRemaining;
            return current > PurchasePrice ? PurchasePrice : current;
        }

        public override string GetMaintenanceStatus()
        {
            var daysLeft = (SubscriptionEndDate - DateTime.UtcNow).TotalDays;
            if (daysLeft <= 0) return "Expired - Renew Now";
            if (daysLeft <= 30) return "Expiring Soon";
            return "Active";
        }
    }
}
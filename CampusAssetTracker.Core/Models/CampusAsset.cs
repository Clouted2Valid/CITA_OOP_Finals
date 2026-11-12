using System;
using CampusAssetTracker.Core.Exceptions;

namespace CampusAssetTracker.Core.Models
{
    public enum AssetStatus
    {
        Available,
        InUse,
        InRepair,
        Retired
    }

    public abstract class CampusAsset
    {
        // Encapsulation: private backing fields, validated setters
        private string _assetTag = string.Empty;
        private string _name = string.Empty;
        private decimal _purchasePrice;
        private AssetStatus _status;

        public string AssetTag
        {
            get => _assetTag;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new AssetException("Asset tag cannot be empty.");
                _assetTag = value;
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new AssetException("Asset name cannot be empty.");
                _name = value;
            }
        }

        public decimal PurchasePrice
        {
            get => _purchasePrice;
            protected set
            {
                if (value < 0)
                    throw new AssetException("Purchase price cannot be negative.");
                _purchasePrice = value;
            }
        }

        public DateTime PurchaseDate { get; protected set; }

        public AssetStatus Status
        {
            get => _status;
            protected set => _status = value;
        }

        protected CampusAsset(string assetTag, string name, decimal purchasePrice, DateTime purchaseDate)
        {
            AssetTag = assetTag;
            Name = name;
            PurchasePrice = purchasePrice;
            PurchaseDate = purchaseDate;
            _status = AssetStatus.Available;
        }

        // Polymorphism: derived classes override this
        public abstract decimal CalculateDepreciation();
        public abstract string GetMaintenanceStatus();

        // Controlled state change
        public virtual void SendToRepair()
        {
            if (_status == AssetStatus.Retired)
                throw new AssetStateException("Retired assets cannot be sent to repair.");
            _status = AssetStatus.InRepair;
        }

        public virtual void ReturnFromRepair()
        {
            if (_status != AssetStatus.InRepair)
                throw new AssetStateException("Asset is not in repair.");
            _status = AssetStatus.Available;
        }

        public virtual void Retire()
        {
            _status = AssetStatus.Retired;
        }

        public override string ToString()
        {
            return $"[{AssetTag}] {Name} ({GetType().Name}) - {Status}";
        }
    }
}
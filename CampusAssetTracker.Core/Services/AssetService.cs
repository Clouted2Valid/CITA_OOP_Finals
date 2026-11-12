using System;
using System.Collections.Generic;
using System.Linq;
using CampusAssetTracker.Core.Exceptions;
using CampusAssetTracker.Core.Interfaces;
using CampusAssetTracker.Core.Models;

namespace CampusAssetTracker.Core.Services
{
    public class AssetService
    {
        private readonly Dictionary<string, CampusAsset> _assets = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyCollection<CampusAsset> GetAll() => _assets.Values.ToList();

        public CampusAsset GetByTag(string tag)
        {
            if (!_assets.TryGetValue(tag, out var asset))
                throw new AssetNotFoundException(tag);
            return asset;
        }

        public void Add(CampusAsset asset)
        {
            if (_assets.ContainsKey(asset.AssetTag))
                throw new AssetException($"Asset tag '{asset.AssetTag}' already exists.");
            _assets[asset.AssetTag] = asset;
        }

        public bool Remove(string tag) => _assets.Remove(tag);

        // -------- LINQ Business Logic --------

        public List<CampusAsset> SearchByName(string keyword)
        {
            return _assets.Values
                .Where(a => a.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<CampusAsset> FilterByStatus(AssetStatus status)
        {
            return _assets.Values.Where(a => a.Status == status).ToList();
        }

        public List<CampusAsset> FilterByType<T>() where T : CampusAsset
        {
            return _assets.Values.OfType<T>().Cast<CampusAsset>().ToList();
        }

        // Interface casting + LINQ: all borrowable items currently checked out
        public List<IBorrowable> GetCheckedOutBorrowables()
        {
            return _assets.Values
                .OfType<IBorrowable>()
                .Where(b => b.IsCheckedOut)
                .ToList();
        }

        public List<INetworkable> GetNetworkDevices()
        {
            return _assets.Values.OfType<INetworkable>().ToList();
        }

        public decimal GetTotalDepreciation()
        {
            return _assets.Values.Sum(a => a.CalculateDepreciation());
        }

        public Dictionary<string, int> GetStatusSummary()
        {
            return _assets.Values
                .GroupBy(a => a.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
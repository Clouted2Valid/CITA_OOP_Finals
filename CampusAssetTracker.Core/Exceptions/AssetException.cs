using System;

namespace CampusAssetTracker.Core.Exceptions
{
    public class AssetException : Exception
    {
        public AssetException(string message) : base(message) { }
    }

    public class AssetNotFoundException : AssetException
    {
        public AssetNotFoundException(string assetTag)
            : base($"Asset with tag '{assetTag}' was not found.") { }
    }

    public class AssetStateException : AssetException
    {
        public AssetStateException(string message) : base(message) { }
    }
}
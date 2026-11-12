namespace CampusAssetTracker.Core.Interfaces
{
    public interface INetworkable
    {
        string? IpAddress { get; }
        string? MacAddress { get; }

        void AssignNetwork(string ipAddress, string macAddress);
    }
}
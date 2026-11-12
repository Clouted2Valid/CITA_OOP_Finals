namespace CampusAssetTracker.Core.Interfaces
{
    public interface IBorrowable
    {
        bool IsCheckedOut { get; }
        string? BorrowerId { get; }

        void CheckOut(string borrowerId);
        void CheckIn();
    }
}
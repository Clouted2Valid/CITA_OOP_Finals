using System;
using CampusAssetTracker.Core.Exceptions;
using CampusAssetTracker.Core.Interfaces;
using CampusAssetTracker.Core.Models;
using CampusAssetTracker.Core.Services;

namespace CampusAssetTracker.CLI
{
    public class Program
    {
        private static readonly AssetService Service = new();

        private static void Main()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("===== Campus IT Asset Tracker =====");
                Console.WriteLine("1. List all assets");
                Console.WriteLine("2. Add asset");
                Console.WriteLine("3. Check out item");
                Console.WriteLine("4. Check in item");
                Console.WriteLine("5. Send to repair / Return from repair");
                Console.WriteLine("6. Search assets by name");
                Console.WriteLine("7. Filter by status");
                Console.WriteLine("8. Show checked-out items (LINQ)");
                Console.WriteLine("9. Show maintenance status report");
                Console.WriteLine("10. Total depreciation");
                Console.WriteLine("11. Show assets of a specific type");
                Console.WriteLine("0. Exit");
                Console.Write("Choose: ");

                switch (Console.ReadLine())
                {
                    case "1": ListAll(); break;
                    case "2": AddAsset(); break;
                    case "3": CheckOut(); break;
                    case "4": CheckIn(); break;
                    case "5": RepairMenu(); break;
                    case "6": Search(); break;
                    case "7": FilterStatus(); break;
                    case "8": ShowCheckedOut(); break;
                    case "9": ShowMaintenanceReport(); break;
                    case "10": ShowDepreciation(); break;
                    case "11": FilterByType(); break;
                    case "0": running = false; break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to try again...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private static void ListAll()
        {
            Console.Clear();
            var all = Service.GetAll();
            if (all.Count == 0)
            {
                Console.WriteLine("No assets found.");
            }
            else
            {
                foreach (var a in all)
                    Console.WriteLine(a);
            }
            Pause();
        }

        private static void AddAsset()
        {
            Console.Clear();
            Console.WriteLine("=== Add Asset ===");
            Console.WriteLine("1 = Workstation");
            Console.WriteLine("2 = Networking Hardware");
            Console.WriteLine("3 = Peripheral");
            Console.WriteLine("4 = Software License");
            Console.Write("Type: ");
            var type = Console.ReadLine();

            Console.Write("Asset Tag: ");
            var tag = Console.ReadLine() ?? string.Empty;
            Console.Write("Name: ");
            var name = Console.ReadLine() ?? string.Empty;
            Console.Write("Purchase Price: ");
            var priceInput = Console.ReadLine();

            if (!decimal.TryParse(priceInput, out var price) || price < 0)
            {
                Console.WriteLine("Invalid price.");
                Pause();
                return;
            }

            try
            {
                CampusAsset asset = type switch
                {
                    "1" => new Workstation(tag, name, price, DateTime.UtcNow, "Generic CPU"),
                    "2" => new NetworkingHardware(tag, name, price, DateTime.UtcNow),
                    "3" => new Peripheral(tag, name, price, DateTime.UtcNow),
                    "4" => new SoftwareLicense(tag, name, price, DateTime.UtcNow, 12, DateTime.UtcNow.AddMonths(12)),
                    _ => throw new AssetException("Unknown asset type.")
                };
                Service.Add(asset);
                Console.WriteLine("Asset added successfully.");
            }
            catch (AssetException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Pause();
        }

        private static void CheckOut()
        {
            Console.Clear();
            Console.WriteLine("=== Check Out ===");
            Console.Write("Asset Tag: ");
            var tag = Console.ReadLine() ?? string.Empty;
            Console.Write("Borrower ID: ");
            var borrower = Console.ReadLine() ?? string.Empty;

            try
            {
                var asset = Service.GetByTag(tag);
                if (asset is not IBorrowable borrowable)
                {
                    Console.WriteLine("This asset is not borrowable.");
                    Pause();
                    return;
                }
                borrowable.CheckOut(borrower);
                Console.WriteLine($"'{asset.Name}' checked out to {borrower}.");
            }
            catch (AssetException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Pause();
        }

        private static void CheckIn()
        {
            Console.Clear();
            Console.WriteLine("=== Check In ===");
            Console.Write("Asset Tag: ");
            var tag = Console.ReadLine() ?? string.Empty;

            try
            {
                var asset = Service.GetByTag(tag);
                if (asset is not IBorrowable borrowable)
                {
                    Console.WriteLine("This asset is not borrowable.");
                    Pause();
                    return;
                }
                borrowable.CheckIn();
                Console.WriteLine($"'{asset.Name}' checked in.");
            }
            catch (AssetException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Pause();
        }

        private static void RepairMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Repair Menu ===");
            Console.Write("Asset Tag: ");
            var tag = Console.ReadLine() ?? string.Empty;
            Console.Write("(1) Send to repair  (2) Return from repair: ");
            var action = Console.ReadLine();

            try
            {
                var asset = Service.GetByTag(tag);
                if (action == "1")
                {
                    asset.SendToRepair();
                    Console.WriteLine($"'{asset.Name}' sent to repair.");
                }
                else if (action == "2")
                {
                    asset.ReturnFromRepair();
                    Console.WriteLine($"'{asset.Name}' returned from repair.");
                }
                else
                {
                    Console.WriteLine("Invalid action.");
                }
            }
            catch (AssetException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Pause();
        }

        private static void Search()
        {
            Console.Clear();
            Console.WriteLine("=== Search by Name ===");
            Console.Write("Keyword: ");
            var keyword = Console.ReadLine() ?? string.Empty;

            var results = Service.SearchByName(keyword);
            if (results.Count == 0)
            {
                Console.WriteLine("No matching assets.");
            }
            else
            {
                foreach (var a in results)
                    Console.WriteLine(a);
            }
            Pause();
        }

        private static void FilterStatus()
        {
            Console.Clear();
            Console.WriteLine("=== Filter by Status ===");
            Console.WriteLine("Options: Available, InUse, InRepair, Retired");
            Console.Write("Status: ");
            var input = Console.ReadLine() ?? string.Empty;

            if (!Enum.TryParse<AssetStatus>(input, true, out var status))
            {
                Console.WriteLine("Invalid status.");
                Pause();
                return;
            }

            var results = Service.FilterByStatus(status);
            if (results.Count == 0)
            {
                Console.WriteLine("No assets with that status.");
            }
            else
            {
                foreach (var a in results)
                    Console.WriteLine(a);
            }
            Pause();
        }

        private static void FilterByType()
        {
            Console.Clear();
            Console.WriteLine("=== Filter by Type ===");
            Console.WriteLine("1 = Workstation");
            Console.WriteLine("2 = Networking Hardware");
            Console.WriteLine("3 = Peripheral");
            Console.WriteLine("4 = Software License");
            Console.Write("Choice: ");
            var choice = Console.ReadLine();

            System.Collections.Generic.List<CampusAsset> results = choice switch
            {
                "1" => Service.FilterByType<Workstation>(),
                "2" => Service.FilterByType<NetworkingHardware>(),
                "3" => Service.FilterByType<Peripheral>(),
                "4" => Service.FilterByType<SoftwareLicense>(),
                _ => new System.Collections.Generic.List<CampusAsset>()
            };

            if (results.Count == 0)
            {
                Console.WriteLine("No assets found for that type.");
            }
            else
            {
                foreach (var a in results)
                    Console.WriteLine(a);
            }
            Pause();
        }

        private static void ShowCheckedOut()
        {
            Console.Clear();
            Console.WriteLine("=== Checked-Out Items (LINQ + Interface Casting) ===");
            var checkedOut = Service.GetCheckedOutBorrowables();

            if (checkedOut.Count == 0)
            {
                Console.WriteLine("Nothing is currently checked out.");
            }
            else
            {
                foreach (var item in checkedOut)
                {
                    if (item is CampusAsset asset)
                        Console.WriteLine($"[{asset.AssetTag}] {asset.Name} - Borrower: {item.BorrowerId}");
                    else
                        Console.WriteLine($"Borrower: {item.BorrowerId}");
                }
            }
            Pause();
        }

        private static void ShowMaintenanceReport()
        {
            Console.Clear();
            Console.WriteLine("=== Maintenance Status Report (Polymorphism) ===");

            foreach (var a in Service.GetAll())
                Console.WriteLine($"{a.AssetTag,-10} {a.Name,-30} -> {a.GetMaintenanceStatus()}");

            Pause();
        }

        private static void ShowDepreciation()
        {
            Console.Clear();
            Console.WriteLine("=== Total Depreciation (Polymorphism) ===");
            Console.WriteLine($"Total current value of all assets: {Service.GetTotalDepreciation():C}");
            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to return to the menu...");
            Console.ReadKey();
        }
    }
}
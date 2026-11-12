using CampusAssetTracker.Core.Exceptions;
using CampusAssetTracker.Core.Interfaces;
using CampusAssetTracker.Core.Models;
using CampusAssetTracker.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusAssetTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AssetService _service;
        public AssetsController(AssetService service) => _service = service;

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("{tag}")]
        public IActionResult Get(string tag)
        {
            try { return Ok(_service.GetByTag(tag)); }
            catch (AssetNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost]
        public IActionResult Add([FromBody] AddAssetRequest req)
        {
            try
            {
                CampusAsset asset = req.Type switch
                {
                    "Workstation" => new Workstation(req.AssetTag, req.Name, req.Price, DateTime.UtcNow, req.CpuSpec ?? "N/A"),
                    "Networking" => new NetworkingHardware(req.AssetTag, req.Name, req.Price, DateTime.UtcNow),
                    "Peripheral" => new Peripheral(req.AssetTag, req.Name, req.Price, DateTime.UtcNow),
                    "Software" => new SoftwareLicense(req.AssetTag, req.Name, req.Price, DateTime.UtcNow, 12, DateTime.UtcNow.AddMonths(12)),
                    _ => throw new AssetException("Invalid type")
                };
                _service.Add(asset);
                return CreatedAtAction(nameof(Get), new { tag = asset.AssetTag }, asset);
            }
            catch (AssetException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("{tag}/checkout")]
        public IActionResult CheckOut(string tag, [FromBody] BorrowerRequest req)
        {
            try
            {
                var asset = _service.GetByTag(tag);
                if (asset is not IBorrowable b) return BadRequest("Not borrowable.");
                b.CheckOut(req.BorrowerId);
                return Ok(new { message = "Checked out", tag, borrower = req.BorrowerId });
            }
            catch (AssetException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("{tag}/checkin")]
        public IActionResult CheckIn(string tag)
        {
            try
            {
                var asset = _service.GetByTag(tag);
                if (asset is not IBorrowable b) return BadRequest("Not borrowable.");
                b.CheckIn();
                return Ok(new { message = "Checked in", tag });
            }
            catch (AssetException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("checkedout")]
        public IActionResult GetCheckedOut()
        {
            var list = _service.GetCheckedOutBorrowables();
            return Ok(list.Select(b => new { b.BorrowerId, IsCheckedOut = b.IsCheckedOut }));
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string q) => Ok(_service.SearchByName(q));

        [HttpGet("filter/{status}")]
        public IActionResult Filter(string status)
        {
            if (!Enum.TryParse<AssetStatus>(status, true, out var s))
                return BadRequest("Invalid status");
            return Ok(_service.FilterByStatus(s));
        }

        [HttpGet("depreciation")]
        public IActionResult TotalDepreciation() => Ok(new { total = _service.GetTotalDepreciation() });

        [HttpGet("status-summary")]
        public IActionResult StatusSummary() => Ok(_service.GetStatusSummary());

        [HttpGet("network-devices")]
        public IActionResult NetworkDevices() => Ok(_service.GetNetworkDevices()
            .Select(n => new { n.IpAddress, n.MacAddress }));

        [HttpPost("{tag}/repair")]
        public IActionResult Repair(string tag, [FromQuery] bool returnFromRepair = false)
        {
            try
            {
                var asset = _service.GetByTag(tag);
                if (returnFromRepair) asset.ReturnFromRepair();
                else asset.SendToRepair();
                return Ok(new { message = returnFromRepair ? "Returned from repair" : "Sent to repair", tag });
            }
            catch (AssetException ex) { return BadRequest(ex.Message); }
        }
    }

    public record AddAssetRequest(string AssetTag, string Name, decimal Price, string Type, string? CpuSpec);
    public record BorrowerRequest(string BorrowerId);
}
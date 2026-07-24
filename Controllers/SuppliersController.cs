using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.API.Authorization;
using POS.API.Constants;
using POS.API.Models;
using POS.API.Services.Contracts;

namespace POS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _svc;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ISupplierService svc, ILogger<SuppliersController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    private string BranchId => User.FindFirstValue("branchId") ?? "hq";

    [HttpGet]
    [RequirePermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("GET /api/suppliers called for branch {BranchId}", BranchId);
        var r = await _svc.GetSuppliersAsync(BranchId);
        return Ok(r.Payload);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Suppliers.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/suppliers/{SupplierId} called", id);
        var r = await _svc.GetByIdAsync(id);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(r.Payload);
    }

    [HttpPost]
    [RequirePermission(Permissions.Suppliers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        _logger.LogInformation("POST /api/suppliers called for branch {BranchId}, name {SupplierName}", BranchId, request.Name);
        var r = await _svc.CreateAsync(BranchId, request);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return CreatedAtAction(nameof(GetById), new { id = r.Payload!.Id }, r.Payload);
    }

    [HttpGet("{id:guid}/orders")]
    [RequirePermission(Permissions.Suppliers.ManageOrders)]
    public async Task<IActionResult> GetOrders(Guid id)
    {
        _logger.LogInformation("GET /api/suppliers/{SupplierId}/orders called", id);
        var r = await _svc.GetOrdersAsync(id);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(r.Payload);
    }

    [HttpPost("{id:guid}/orders")]
    [RequirePermission(Permissions.Suppliers.ManageOrders)]
    public async Task<IActionResult> CreateOrder(Guid id, [FromBody] CreatePurchaseOrderRequest request)
    {
        _logger.LogInformation("POST /api/suppliers/{SupplierId}/orders called for branch {BranchId}, {ItemCount} item(s)", id, BranchId, request.Items.Count);
        var r = await _svc.CreateOrderAsync(id, BranchId, request);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(r.Payload);
    }

    [HttpPost("orders/{orderId:guid}/send")]
    [RequirePermission(Permissions.Suppliers.ManageOrders)]
    public async Task<IActionResult> SendOrder(Guid orderId)
    {
        _logger.LogInformation("POST /api/suppliers/orders/{OrderId}/send called", orderId);
        var r = await _svc.SendOrderAsync(orderId);
        if (!r.Success) return StatusCode(r.StatusCode, new { error = r.ErrorMessage });
        return Ok(new { success = true });
    }
}

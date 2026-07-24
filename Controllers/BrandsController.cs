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
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(IBrandService brandService, ILogger<BrandsController> logger)
    {
        _brandService = brandService;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission(Permissions.Inventory.View, Permissions.Inventory.ViewStockOnly)]
    public async Task<IActionResult> GetBrands()
    {
        _logger.LogInformation("GET /api/brands called");
        var result = await _brandService.GetBrandsAsync();
        return Ok(result.Payload);
    }

    [HttpPost]
    [RequirePermission(Permissions.Inventory.AddStock)]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
    {
        _logger.LogInformation("POST /api/brands called, name {Name}", request.Name);
        var result = await _brandService.CreateAsync(request);
        if (!result.Success) return StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        return Ok(result.Payload);
    }
}

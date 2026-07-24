using POS.API.Models;
using POS.API.Repositories.Interfaces;
using POS.API.Services.Contracts;

namespace POS.API.Services.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepo;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IBrandRepository brandRepo, ILogger<BrandService> logger)
    {
        _brandRepo = brandRepo;
        _logger = logger;
    }

    public async Task<OperationResult<List<BrandDto>>> GetBrandsAsync()
    {
        var brands = await _brandRepo.GetAllActiveAsync();
        return OperationResult<List<BrandDto>>.Ok(brands.Select(ToDto).ToList());
    }

    public async Task<OperationResult<BrandDto>> CreateAsync(CreateBrandRequest request)
    {
        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult<BrandDto>.Fail("Brand name is required.");

        var existing = await _brandRepo.GetByNameAsync(name);
        if (existing is not null)
            return OperationResult<BrandDto>.Ok(ToDto(existing));

        var brand = new Brand { Name = name };
        var saved = await _brandRepo.AddAsync(brand);
        _logger.LogInformation("Brand {Name} created with id {Id}", saved.Name, saved.Id);
        return OperationResult<BrandDto>.Ok(ToDto(saved));
    }

    private static BrandDto ToDto(Brand b) => new() { Id = b.Id, Name = b.Name };
}

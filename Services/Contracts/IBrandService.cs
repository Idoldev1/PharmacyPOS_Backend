using POS.API.Models;

namespace POS.API.Services.Contracts;

public interface IBrandService
{
    Task<OperationResult<List<BrandDto>>> GetBrandsAsync();
    Task<OperationResult<BrandDto>> CreateAsync(CreateBrandRequest request);
}

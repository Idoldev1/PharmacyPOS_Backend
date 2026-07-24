namespace POS.API.Models;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

public class CreateBrandRequest
{
    public string Name { get; set; } = null!;
}

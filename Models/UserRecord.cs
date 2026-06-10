using Microsoft.AspNetCore.Identity;
using POS.API.Models;

namespace POS.API.Models;

public class UserRecord : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string BranchId { get; set; } = null!;
}
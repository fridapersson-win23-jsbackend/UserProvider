using Data.Entities;

namespace UserProvider.Models;

public class UserUpdateModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set;} = null!;
    public string? Email { get; set; }
    public string? Biography { get; set; }
    public string? PhoneNumber { get; set; }
    public int? AddressId { get; set; }
    public AddressEntity? Address { get; set; }
}

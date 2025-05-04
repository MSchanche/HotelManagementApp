using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class RoleTypes
{
    [Key]
    public int RoleTypeID { get; set; }
    public string RoleTypeName { get; set; }
    
    public ICollection<Account> Accounts { get; set; }
}
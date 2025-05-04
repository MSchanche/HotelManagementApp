using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class Account
{
    [Key]
    public int AccountID { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; }
    public int RoleTypeID { get; set; }
    public int HotelID { get; set; }
    
    public RoleTypes RoleTypes { get; set; }
    public Hotel Hotel { get; set; }
    public ICollection<Bookings> Bookings { get; set; }
    public ICollection<Messages> Messages { get; set; }
    public ICollection<MessageEntries> MessageEntries { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;



public class Hotel
{
    [Key]
    public int HotelID { get; set; }
    public string HotelName { get; set; }
    public string HotelAddress { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    
    // Navigation properties
    public ICollection<Room> Rooms { get; set; }
    public ICollection<Account> Accounts { get; set; }
}
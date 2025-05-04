using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class BookingStatusTypes
{
    [Key]
    public int BookingStatusID { get; set; }
    public string BookingStatusName { get; set; }
    
    public ICollection<Bookings> Bookings { get; set; }
}
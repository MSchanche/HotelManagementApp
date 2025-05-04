using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class Bookings
{
    [Key]
    public int BookingID { get; set; }
    public int AccountID { get; set; }
    public int RoomID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int BookingStatusID { get; set; }
    
    public Account Account { get; set; }
    public Room Room { get; set; }
    public BookingStatusTypes BookingStatusTypes { get; set; }
    public ICollection<Messages> Messages { get; set; }
}
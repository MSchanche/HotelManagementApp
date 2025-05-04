using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class Messages
{
    [Key]
    public int MessageID { get; set; }
    public int? BookingID { get; set; }
    public int RoomID { get; set; }
    public int? AccountID { get; set; }
    public int WorkTypeID { get; set; }
    public int MessageStatusID { get; set; }
    public DateTime CreationTime { get; set; }
    
    public Bookings Booking { get; set; }
    public Room Room { get; set; }
    public Account Account { get; set; }
    public WorkTypes WorkTypes { get; set; }
    public MessageStatusTypes MessageStatusTypes { get; set; }
    public ICollection<MessageEntries> MessageEntries { get; set; }
}
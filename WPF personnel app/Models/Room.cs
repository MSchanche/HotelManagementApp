using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class Room
{
    [Key]
    public int RoomID { get; set; }
    public int HotelID { get; set; }
    public string RoomName { get; set; }
    public int RoomTypeID { get; set; }
    public decimal RoomPrice { get; set; }
    public string Description { get; set; }

    // Navigation properties
    public Hotel Hotel { get; set; }
    public RoomTypes RoomTypes { get; set; }
    public ICollection<Bookings> Bookings { get; set; }
    public ICollection<Messages> Messages { get; set; }
}
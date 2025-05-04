using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class RoomTypes
{
    [Key]
    public int RoomTypeID { get; set; }
    public string RoomTypeName { get; set; }
    
    // Navigation property
    public ICollection<Room> Rooms { get; set; }
}
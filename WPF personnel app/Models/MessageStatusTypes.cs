using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class MessageStatusTypes
{
    [Key]
    public int MessageStatusID { get; set; }
    public string MessageStatusName { get; set; }
    
    public ICollection<Messages> Messages { get; set; }
}
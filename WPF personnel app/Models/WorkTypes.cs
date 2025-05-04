using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class WorkTypes
{
    [Key]
    public int WorkTypeID { get; set; }
    public string WorkTypeName { get; set; }
    
    public ICollection<Messages> Messages { get; set; }
}
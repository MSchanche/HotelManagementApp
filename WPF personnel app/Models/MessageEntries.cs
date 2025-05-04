using System.ComponentModel.DataAnnotations;

namespace HotelRegistrationWebsite.Models;

public class MessageEntries
{
    [Key]
    public int MessageEntryID { get; set; }
    public int MessageID { get; set; }
    public int? AccountID { get; set; }
    public DateTime CreationTime { get; set; }
    public string EntryContent { get; set; }
    
    public Messages Message { get; set; }
    public Account Account { get; set; }
}
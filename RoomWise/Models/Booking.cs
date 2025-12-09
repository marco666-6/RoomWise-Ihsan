// Models/Booking.cs
namespace RoomWise.Models
{
    public class Booking
    {
        public int bkg_id { get; set; }
        public int bkg_rom_id { get; set; }
        public int bkg_usr_id { get; set; }
        public DateTime bkg_dt { get; set; }
        public TimeSpan bkg_start_time { get; set; }
        public TimeSpan bkg_end_time { get; set; }
        public string? bkg_purpose { get; set; }
        public byte? bkg_attendees { get; set; }
        public string? bkg_status { get; set; }
        public DateTime? bkg_cancelled_dt { get; set; }
        public string? bkg_cancelled_reason { get; set; }
        public DateTime bkg_created_dt { get; set; }
        public int bkg_created_by { get; set; }
        public DateTime? bkg_updated_dt { get; set; }
        public int? bkg_updated_by { get; set; }
        
        // Navigation properties
        public string? rom_name { get; set; }
        public string? usr_name { get; set; }
    }
}
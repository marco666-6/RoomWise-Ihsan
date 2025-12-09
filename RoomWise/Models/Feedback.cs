// Models/Feedback.cs
namespace RoomWise.Models
{
    public class Feedback
    {
        public int fdb_id { get; set; }
        public int fdb_bkg_id { get; set; }
        public int fdb_rom_id { get; set; }
        public int fdb_usr_id { get; set; }
        public byte fdb_rating { get; set; }
        public string? fdb_comment { get; set; }
        public DateTime fdb_created_dt { get; set; }
        
        // Navigation properties
        public string? rom_name { get; set; }
        public string? usr_name { get; set; }
    }
    
    public class FeedbackPhoto
    {
        public int fph_id { get; set; }
        public int fph_fdb_id { get; set; }
        public string? fph_photo_path { get; set; }
        public int fph_photo_size_kb { get; set; }
        public DateTime fph_uploaded_dt { get; set; }
    }
}
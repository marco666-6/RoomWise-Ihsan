// Models/Room.cs
namespace RoomWise.Models
{
    public class Room
    {
        public int rom_id { get; set; }
        public string? rom_name { get; set; }
        public byte rom_capacity { get; set; }
        public bool rom_has_projector { get; set; }
        public bool rom_has_whiteboard { get; set; }
        public bool rom_has_video_conf { get; set; }
        public string? rom_status { get; set; }
        public string? rom_status_reason { get; set; }
        public DateTime? rom_status_updated_dt { get; set; }
        public int? rom_status_updated_by { get; set; }
        public bool rom_is_active { get; set; }
        public DateTime rom_created_dt { get; set; }
    }
}
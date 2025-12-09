// Models/Notification.cs
namespace RoomWise.Models
{
    public class Notification
    {
        public int ntf_id { get; set; }
        public int ntf_usr_id { get; set; }
        public string? ntf_type { get; set; }
        public string? ntf_title { get; set; }
        public string? ntf_message { get; set; }
        public string? ntf_related_entity { get; set; }
        public int? ntf_related_id { get; set; }
        public bool ntf_is_read { get; set; }
        public DateTime ntf_created_dt { get; set; }
        public DateTime? ntf_read_dt { get; set; }
    }
    
    public class RoomStatusHistory
    {
        public int rsh_id { get; set; }
        public int rsh_rom_id { get; set; }
        public string? rsh_old_status { get; set; }
        public string? rsh_new_status { get; set; }
        public string? rsh_reason { get; set; }
        public int rsh_changed_by { get; set; }
        public DateTime rsh_changed_dt { get; set; }
    }
    
    public class AuditLog
    {
        public int aud_id { get; set; }
        public int? aud_usr_id { get; set; }
        public string? aud_action { get; set; }
        public string? aud_entity { get; set; }
        public int? aud_entity_id { get; set; }
        public string? aud_description { get; set; }
        public string? aud_ip_address { get; set; }
        public DateTime aud_dt { get; set; }
    }
}
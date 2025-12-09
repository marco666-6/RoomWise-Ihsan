// Models/User.cs
namespace RoomWise.Models
{
    public class User
    {
        public int usr_id { get; set; }
        public string? usr_name { get; set; }
        public string? usr_email { get; set; }
        public string? usr_password { get; set; }
        public string? usr_role { get; set; }
        public string? usr_dept { get; set; }
        public bool usr_is_active { get; set; }
        public DateTime usr_created_dt { get; set; }
        public DateTime? usr_updated_dt { get; set; }
    }
}
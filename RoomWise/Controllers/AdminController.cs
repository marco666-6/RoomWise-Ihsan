// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RoomWise.Functions;
using RoomWise.Models;

namespace RoomWise.Controllers
{
    public class AdminController : Controller
    {
        private readonly DbAccess _db;
        
        public AdminController()
        {
            _db = new DbAccess();
        }
        
        // Check if user is admin
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
        
        // Dashboard
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.TotalUsers = GetCount("SELECT COUNT(*) FROM rws_users WHERE usr_is_active = 1");
            ViewBag.TotalRooms = GetCount("SELECT COUNT(*) FROM rws_rooms WHERE rom_is_active = 1");
            ViewBag.TodayBookings = GetCount("SELECT COUNT(*) FROM rws_bookings WHERE bkg_dt = CAST(GETDATE() AS DATE) AND bkg_status = 'Upcoming'");
            ViewBag.ActiveBookings = GetCount("SELECT COUNT(*) FROM rws_bookings WHERE bkg_status = 'Upcoming'");
            
            return View();
        }
        
        // Users Management
        public IActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            List<User> users = new List<User>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT * FROM rws_users ORDER BY usr_created_dt DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            usr_id = reader.GetInt32(0),
                            usr_name = reader.GetString(1),
                            usr_email = reader.GetString(2),
                            usr_role = reader.GetString(4),
                            usr_dept = reader.IsDBNull(5) ? null : reader.GetString(5),
                            usr_is_active = reader.GetBoolean(6),
                            usr_created_dt = reader.GetDateTime(7)
                        });
                    }
                }
            }
            
            return View(users);
        }
        
        // Add User
        [HttpPost]
        public IActionResult AddUser(string name, string email, string password, string role, string dept)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                string hashedPassword = SecHelper.HashPasswordMD5(password);
                
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"INSERT INTO rws_users (usr_name, usr_email, usr_password, usr_role, usr_dept)
                                   VALUES (@name, @email, @password, @role, @dept)";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        cmd.Parameters.AddWithValue("@role", role);
                        cmd.Parameters.AddWithValue("@dept", string.IsNullOrEmpty(dept) ? DBNull.Value : dept);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("CREATE", "USER", null, $"Created user: {name}");
                TempData["Success"] = "User added successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Users");
        }
        
        // Edit User
        [HttpPost]
        public IActionResult EditUser(int id, string name, string email, string role, string dept, bool isActive)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"UPDATE rws_users 
                                   SET usr_name = @name, usr_email = @email, usr_role = @role, 
                                       usr_dept = @dept, usr_is_active = @isActive, usr_updated_dt = GETDATE()
                                   WHERE usr_id = @id";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@role", role);
                        cmd.Parameters.AddWithValue("@dept", string.IsNullOrEmpty(dept) ? DBNull.Value : dept);
                        cmd.Parameters.AddWithValue("@isActive", isActive);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("UPDATE", "USER", id, $"Updated user: {name}");
                TempData["Success"] = "User updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Users");
        }
        
        // Delete User
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = "UPDATE rws_users SET usr_is_active = 0 WHERE usr_id = @id";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("DELETE", "USER", id, "Deactivated user");
                TempData["Success"] = "User deactivated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Users");
        }
        
        // Rooms Management
        public IActionResult Rooms()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            List<Room> rooms = new List<Room>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT * FROM rws_rooms ORDER BY rom_name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new Room
                        {
                            rom_id = reader.GetInt32(0),
                            rom_name = reader.GetString(1),
                            rom_capacity = reader.GetByte(2),
                            rom_has_projector = reader.GetBoolean(3),
                            rom_has_whiteboard = reader.GetBoolean(4),
                            rom_has_video_conf = reader.GetBoolean(5),
                            rom_status = reader.GetString(6),
                            rom_status_reason = reader.IsDBNull(7) ? null : reader.GetString(7),
                            rom_is_active = reader.GetBoolean(10)
                        });
                    }
                }
            }
            
            return View(rooms);
        }
        
        // Add Room
        [HttpPost]
        public IActionResult AddRoom(string name, byte capacity, bool projector, bool whiteboard, bool videoconf)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"INSERT INTO rws_rooms (rom_name, rom_capacity, rom_has_projector, rom_has_whiteboard, rom_has_video_conf)
                                   VALUES (@name, @capacity, @projector, @whiteboard, @videoconf)";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@capacity", capacity);
                        cmd.Parameters.AddWithValue("@projector", projector);
                        cmd.Parameters.AddWithValue("@whiteboard", whiteboard);
                        cmd.Parameters.AddWithValue("@videoconf", videoconf);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("CREATE", "ROOM", null, $"Created room: {name}");
                TempData["Success"] = "Room added successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Rooms");
        }
        
        // Edit Room
        [HttpPost]
        public IActionResult EditRoom(int id, string name, byte capacity, bool projector, bool whiteboard, bool videoconf)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"UPDATE rws_rooms 
                                   SET rom_name = @name, rom_capacity = @capacity, 
                                       rom_has_projector = @projector, rom_has_whiteboard = @whiteboard,
                                       rom_has_video_conf = @videoconf
                                   WHERE rom_id = @id";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@capacity", capacity);
                        cmd.Parameters.AddWithValue("@projector", projector);
                        cmd.Parameters.AddWithValue("@whiteboard", whiteboard);
                        cmd.Parameters.AddWithValue("@videoconf", videoconf);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("UPDATE", "ROOM", id, $"Updated room: {name}");
                TempData["Success"] = "Room updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Rooms");
        }
        
        // Update Room Status
        [HttpPost]
        public IActionResult UpdateRoomStatus(int id, string status, string reason)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                int? userIdNullable = HttpContext.Session.GetInt32("UserId");
                if (!userIdNullable.HasValue)
                {
                    TempData["Error"] = "User session not found";
                    return RedirectToAction("Index", "Auth");
                }
                int userId = userIdNullable.Value;
                
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    
                    // Get old status
                    string oldStatus = "";
                    string query1 = "SELECT rom_status FROM rws_rooms WHERE rom_id = @id";
                    using (SqlCommand cmd1 = new SqlCommand(query1, conn))
                    {
                        cmd1.Parameters.AddWithValue("@id", id);
                        oldStatus = cmd1.ExecuteScalar()?.ToString() ?? "";
                    }
                    
                    // Update room status
                    string query2 = @"UPDATE rws_rooms 
                                    SET rom_status = @status, rom_status_reason = @reason,
                                        rom_status_updated_dt = GETDATE(), rom_status_updated_by = @userId
                                    WHERE rom_id = @id";
                    
                    using (SqlCommand cmd2 = new SqlCommand(query2, conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", id);
                        cmd2.Parameters.AddWithValue("@status", status);
                        cmd2.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(reason) ? DBNull.Value : reason);
                        cmd2.Parameters.AddWithValue("@userId", userId);
                        
                        cmd2.ExecuteNonQuery();
                    }
                    
                    // Log history
                    string query3 = @"INSERT INTO rws_room_status_history (rsh_rom_id, rsh_old_status, rsh_new_status, rsh_reason, rsh_changed_by)
                                    VALUES (@romId, @oldStatus, @newStatus, @reason, @userId)";
                    
                    using (SqlCommand cmd3 = new SqlCommand(query3, conn))
                    {
                        cmd3.Parameters.AddWithValue("@romId", id);
                        cmd3.Parameters.AddWithValue("@oldStatus", oldStatus);
                        cmd3.Parameters.AddWithValue("@newStatus", status);
                        cmd3.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(reason) ? DBNull.Value : reason);
                        cmd3.Parameters.AddWithValue("@userId", userId);
                        
                        cmd3.ExecuteNonQuery();
                    }
                }
                
                LogAudit("UPDATE", "ROOM", id, $"Changed room status to {status}");
                TempData["Success"] = "Room status updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Rooms");
        }
        
        // Bookings Management
        public IActionResult Bookings()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            List<Booking> bookings = new List<Booking>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = @"SELECT b.*, r.rom_name, u.usr_name 
                               FROM rws_bookings b
                               INNER JOIN rws_rooms r ON b.bkg_rom_id = r.rom_id
                               INNER JOIN rws_users u ON b.bkg_usr_id = u.usr_id
                               ORDER BY b.bkg_dt DESC, b.bkg_start_time DESC";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookings.Add(new Booking
                        {
                            bkg_id = reader.GetInt32(0),
                            bkg_rom_id = reader.GetInt32(1),
                            bkg_usr_id = reader.GetInt32(2),
                            bkg_dt = reader.GetDateTime(3),
                            bkg_start_time = reader.GetTimeSpan(4),
                            bkg_end_time = reader.GetTimeSpan(5),
                            bkg_purpose = reader.GetString(6),
                            bkg_attendees = reader.IsDBNull(7) ? null : reader.GetByte(7),
                            bkg_status = reader.GetString(8),
                            rom_name = reader.GetString(14),
                            usr_name = reader.GetString(15)
                        });
                    }
                }
            }
            
            // Get rooms for dropdown
            ViewBag.Rooms = GetRooms();
            ViewBag.Users = GetUsers();
            
            return View(bookings);
        }
        
        // Add Booking (Admin can book for others)
        [HttpPost]
        public IActionResult AddBooking(int roomId, int userId, DateTime date, TimeSpan startTime, TimeSpan endTime, string purpose, byte? attendees)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                // Check conflict
                if (CheckBookingConflict(roomId, date, startTime, endTime, null))
                {
                    TempData["Error"] = "Room is already booked for this time slot";
                    return RedirectToAction("Bookings");
                }
                
                int? userIdNullable = HttpContext.Session.GetInt32("UserId");
                if (!userIdNullable.HasValue)
                {
                    TempData["Error"] = "User session not found";
                    return RedirectToAction("Index", "Auth");
                }
                int adminId = userIdNullable.Value;
                
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"INSERT INTO rws_bookings (bkg_rom_id, bkg_usr_id, bkg_dt, bkg_start_time, bkg_end_time, bkg_purpose, bkg_attendees, bkg_created_by)
                                   VALUES (@roomId, @userId, @date, @startTime, @endTime, @purpose, @attendees, @createdBy)";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@roomId", roomId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@date", date);
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        cmd.Parameters.AddWithValue("@purpose", purpose);
                        cmd.Parameters.AddWithValue("@attendees", attendees.HasValue ? attendees.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@createdBy", adminId);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("CREATE", "BOOKING", null, $"Created booking for user ID {userId}");
                TempData["Success"] = "Booking created successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Bookings");
        }
        
        // Cancel Booking
        [HttpPost]
        public IActionResult CancelBooking(int id, string reason)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Auth");
            
            try
            {
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"UPDATE rws_bookings 
                                   SET bkg_status = 'Cancelled', bkg_cancelled_dt = GETDATE(), bkg_cancelled_reason = @reason
                                   WHERE bkg_id = @id";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(reason) ? DBNull.Value : reason);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                LogAudit("UPDATE", "BOOKING", id, "Cancelled booking");
                TempData["Success"] = "Booking cancelled successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("Bookings");
        }
        
        // Helper Methods
        private int GetCount(string query)
        {
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
        
        private List<Room> GetRooms()
        {
            List<Room> rooms = new List<Room>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT rom_id, rom_name FROM rws_rooms WHERE rom_is_active = 1 ORDER BY rom_name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rooms.Add(new Room { rom_id = reader.GetInt32(0), rom_name = reader.GetString(1) });
                    }
                }
            }
            return rooms;
        }
        
        private List<User> GetUsers()
        {
            List<User> users = new List<User>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT usr_id, usr_name FROM rws_users WHERE usr_is_active = 1 ORDER BY usr_name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User { usr_id = reader.GetInt32(0), usr_name = reader.GetString(1) });
                    }
                }
            }
            return users;
        }
        
        private bool CheckBookingConflict(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeBookingId)
        {
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = @"SELECT COUNT(*) FROM rws_bookings
                               WHERE bkg_rom_id = @roomId AND bkg_dt = @date AND bkg_status = 'Upcoming'
                               AND (@excludeId IS NULL OR bkg_id != @excludeId)
                               AND ((@startTime >= bkg_start_time AND @startTime < bkg_end_time)
                                    OR (@endTime > bkg_start_time AND @endTime <= bkg_end_time)
                                    OR (@startTime <= bkg_start_time AND @endTime >= bkg_end_time))";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@roomId", roomId);
                    cmd.Parameters.AddWithValue("@date", date);
                    cmd.Parameters.AddWithValue("@startTime", startTime);
                    cmd.Parameters.AddWithValue("@endTime", endTime);
                    cmd.Parameters.AddWithValue("@excludeId", excludeBookingId.HasValue ? excludeBookingId.Value : DBNull.Value);
                    
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }
        
        private void LogAudit(string action, string entity, int? entityId, string description)
        {
            try
            {
                int? userIdNullable = HttpContext.Session.GetInt32("UserId");
                if (!userIdNullable.HasValue)
                {
                    // Optionally handle missing session, e.g., throw or log
                    return;
                }
                int userId = userIdNullable.Value;
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"INSERT INTO rws_audit_log (aud_usr_id, aud_action, aud_entity, aud_entity_id, aud_description, aud_ip_address)
                                   VALUES (@userId, @action, @entity, @entityId, @description, @ip)";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@action", action);
                        cmd.Parameters.AddWithValue("@entity", entity);
                        cmd.Parameters.AddWithValue("@entityId", entityId.HasValue ? entityId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@description", description);
                        cmd.Parameters.AddWithValue("@ip", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "");
                        
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
    }
}
// Controllers/KaryawanController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RoomWise.Functions;
using RoomWise.Models;

namespace RoomWise.Controllers
{
    public class KaryawanController : Controller
    {
        private readonly DbAccess _db;
        private readonly IWebHostEnvironment _env;
        
        public KaryawanController(IWebHostEnvironment env)
        {
            _db = new DbAccess();
            _env = env;
        }
        
        // Check if user is logged in
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }
        
        // Dashboard
        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            int? userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (!userIdNullable.HasValue)
            {
                TempData["Error"] = "User session not found";
                return RedirectToAction("Index", "Auth");
            }
            int userId = userIdNullable.Value;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.MyUpcomingBookings = GetCount($"SELECT COUNT(*) FROM rws_bookings WHERE bkg_usr_id = {userId} AND bkg_status = 'Upcoming'");
            ViewBag.AvailableRooms = GetCount("SELECT COUNT(*) FROM rws_rooms WHERE rom_is_active = 1 AND rom_status = 'Available'");
            
            // Get today's bookings
            List<Booking> todayBookings = new List<Booking>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = @"SELECT b.*, r.rom_name, u.usr_name 
                               FROM rws_bookings b
                               INNER JOIN rws_rooms r ON b.bkg_rom_id = r.rom_id
                               INNER JOIN rws_users u ON b.bkg_usr_id = u.usr_id
                               WHERE b.bkg_usr_id = @userId AND b.bkg_dt = CAST(GETDATE() AS DATE)
                               AND b.bkg_status = 'Upcoming'
                               ORDER BY b.bkg_start_time";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            todayBookings.Add(new Booking
                            {
                                bkg_id = reader.GetInt32(0),
                                bkg_rom_id = reader.GetInt32(1),
                                bkg_usr_id = reader.GetInt32(2),
                                bkg_dt = reader.GetDateTime(3),
                                bkg_start_time = reader.GetTimeSpan(4),
                                bkg_end_time = reader.GetTimeSpan(5),
                                bkg_purpose = reader.GetString(6),
                                bkg_status = reader.GetString(8),
                                rom_name = reader.GetString(14),
                                usr_name = reader.GetString(15)
                            });
                        }
                    }
                }
            }
            
            ViewBag.TodayBookings = todayBookings;
            return View();
        }
        
        // View Room Status
        public IActionResult Rooms()
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            List<Room> rooms = new List<Room>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT * FROM rws_rooms WHERE rom_is_active = 1 ORDER BY rom_name";
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
                            rom_status_reason = reader.IsDBNull(7) ? null : reader.GetString(7)
                        });
                    }
                }
            }
            
            return View(rooms);
        }
        
        // Book Room
        public IActionResult BookRoom(int? roomId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            ViewBag.Rooms = GetAvailableRooms();
            ViewBag.SelectedRoomId = roomId;
            
            return View();
        }
        
        // Create Booking
        [HttpPost]
        public IActionResult CreateBooking(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime, string purpose, byte? attendees)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            try
            {
                // Validate duration (min 30 min, max 8 hours)
                TimeSpan duration = endTime - startTime;
                if (duration.TotalMinutes < 30)
                {
                    TempData["Error"] = "Booking duration must be at least 30 minutes";
                    return RedirectToAction("BookRoom");
                }
                if (duration.TotalHours > 8)
                {
                    TempData["Error"] = "Booking duration cannot exceed 8 hours";
                    return RedirectToAction("BookRoom");
                }
                
                // Check conflict
                if (CheckBookingConflict(roomId, date, startTime, endTime, null))
                {
                    TempData["Error"] = "Room is already booked for this time slot";
                    return RedirectToAction("BookRoom");
                }
                
                // Validate capacity if attendees specified
                if (attendees.HasValue)
                {
                    byte roomCapacity = GetRoomCapacity(roomId);
                    if (attendees.Value > roomCapacity)
                    {
                        TempData["Error"] = $"Number of attendees ({attendees.Value}) exceeds room capacity ({roomCapacity})";
                        return RedirectToAction("BookRoom");
                    }
                }
                
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
                    string query = @"INSERT INTO rws_bookings (bkg_rom_id, bkg_usr_id, bkg_dt, bkg_start_time, bkg_end_time, bkg_purpose, bkg_attendees, bkg_created_by)
                                   VALUES (@roomId, @userId, @date, @startTime, @endTime, @purpose, @attendees, @userId)";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@roomId", roomId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@date", date);
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        cmd.Parameters.AddWithValue("@purpose", purpose);
                        cmd.Parameters.AddWithValue("@attendees", attendees.HasValue ? attendees.Value : DBNull.Value);
                        
                        cmd.ExecuteNonQuery();
                    }
                }
                
                TempData["Success"] = "Booking created successfully";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("BookRoom");
            }
        }
        
        // My Bookings
        public IActionResult MyBookings()
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");

            int? userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (!userIdNullable.HasValue)
            {
                TempData["Error"] = "User session not found";
                return RedirectToAction("Index", "Auth");
            }
            int userId = userIdNullable.Value;

            List<Booking> bookings = new List<Booking>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = @"SELECT b.*, r.rom_name, u.usr_name 
                                FROM rws_bookings b
                                INNER JOIN rws_rooms r ON b.bkg_rom_id = r.rom_id
                                INNER JOIN rws_users u ON b.bkg_usr_id = u.usr_id
                                WHERE b.bkg_usr_id = @userId
                                ORDER BY b.bkg_dt DESC, b.bkg_start_time DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);

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

                                // NULL-SAFE string fields
                                bkg_purpose = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                bkg_attendees = reader.IsDBNull(7) ? null : reader.GetByte(7),
                                bkg_status = reader.IsDBNull(8) ? "" : reader.GetString(8),

                                // Index 9 = created_by (skip if not used)
                                bkg_cancelled_reason = reader.IsDBNull(10) ? null : reader.GetString(10),

                                // Joined room/user name
                                rom_name = reader.IsDBNull(14) ? "" : reader.GetString(14),
                                usr_name = reader.IsDBNull(15) ? "" : reader.GetString(15)
                            });
                        }
                    }
                }
            }
            return View(bookings);
        }
        
        // Cancel My Booking
        [HttpPost]
        public IActionResult CancelMyBooking(int id, string reason)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            try
            {
                int? userIdNullable = HttpContext.Session.GetInt32("UserId");
                if (!userIdNullable.HasValue)
                {
                    TempData["Error"] = "User session not found";
                    return RedirectToAction("Index", "Auth");
                }
                int userId = userIdNullable.Value;
                
                // Check if booking belongs to user and is upcoming
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    
                    // Get booking details
                    string query1 = "SELECT bkg_usr_id, bkg_dt, bkg_start_time, bkg_status FROM rws_bookings WHERE bkg_id = @id";
                    int bookingUserId = 0;
                    DateTime bookingDate = DateTime.Now;
                    TimeSpan startTime = TimeSpan.Zero;
                    string status = "";
                    
                    using (SqlCommand cmd1 = new SqlCommand(query1, conn))
                    {
                        cmd1.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader reader = cmd1.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bookingUserId = reader.GetInt32(0);
                                bookingDate = reader.GetDateTime(1);
                                startTime = reader.GetTimeSpan(2);
                                status = reader.GetString(3);
                            }
                        }
                    }
                    
                    // Validate ownership
                    if (bookingUserId != userId)
                    {
                        TempData["Error"] = "You can only cancel your own bookings";
                        return RedirectToAction("MyBookings");
                    }
                    
                    // Validate status
                    if (status != "Upcoming")
                    {
                        TempData["Error"] = "Can only cancel upcoming bookings";
                        return RedirectToAction("MyBookings");
                    }
                    
                    // Check time constraint (at least 2 hours before)
                    DateTime bookingDateTime = bookingDate.Add(startTime);
                    if ((bookingDateTime - DateTime.Now).TotalHours < 2)
                    {
                        TempData["Error"] = "Bookings can only be cancelled at least 2 hours before the scheduled time";
                        return RedirectToAction("MyBookings");
                    }
                    
                    // Cancel booking
                    string query2 = @"UPDATE rws_bookings 
                                    SET bkg_status = 'Cancelled', bkg_cancelled_dt = GETDATE(), bkg_cancelled_reason = @reason
                                    WHERE bkg_id = @id";
                    
                    using (SqlCommand cmd2 = new SqlCommand(query2, conn))
                    {
                        cmd2.Parameters.AddWithValue("@id", id);
                        cmd2.Parameters.AddWithValue("@reason", string.IsNullOrEmpty(reason) ? DBNull.Value : reason);
                        
                        cmd2.ExecuteNonQuery();
                    }
                }
                
                TempData["Success"] = "Booking cancelled successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            
            return RedirectToAction("MyBookings");
        }
        
        // Submit Feedback
        public IActionResult Feedback(int bookingId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            int? userIdNullable = HttpContext.Session.GetInt32("UserId");
            if (!userIdNullable.HasValue)
            {
                TempData["Error"] = "User session not found";
                return RedirectToAction("Index", "Auth");
            }
            int userId = userIdNullable.Value;
            
            // Get booking details
            Booking? booking = null;
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = @"SELECT b.*, r.rom_name 
                               FROM rws_bookings b
                               INNER JOIN rws_rooms r ON b.bkg_rom_id = r.rom_id
                               WHERE b.bkg_id = @id AND b.bkg_usr_id = @userId";
                
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", bookingId);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            booking = new Booking
                            {
                                bkg_id = reader.GetInt32(0),
                                bkg_rom_id = reader.GetInt32(1),
                                bkg_dt = reader.GetDateTime(3),
                                bkg_start_time = reader.GetTimeSpan(4),
                                bkg_end_time = reader.GetTimeSpan(5),
                                bkg_purpose = reader.GetString(6),
                                rom_name = reader.GetString(14)
                            };
                        }
                    }
                }
            }
            
            if (booking == null)
            {
                TempData["Error"] = "Booking not found";
                return RedirectToAction("MyBookings");
            }
            
            ViewBag.Booking = booking;
            return View();
        }
        
        // Submit Feedback
        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(int bookingId, int roomId, byte rating, string comment, List<IFormFile> photos)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Auth");
            
            try
            {
                int? userIdNullable = HttpContext.Session.GetInt32("UserId");
                if (!userIdNullable.HasValue)
                {
                    TempData["Error"] = "User session not found";
                    return RedirectToAction("Index", "Auth");
                }
                int userId = userIdNullable.Value;
                
                // Validate photos
                if (photos != null && photos.Count > 3)
                {
                    TempData["Error"] = "Maximum 3 photos allowed";
                    return RedirectToAction("Feedback", new { bookingId });
                }
                
                long totalSize = 0;
                if (photos != null)
                {
                    foreach (var photo in photos)
                    {
                        totalSize += photo.Length;
                    }
                    
                    if (totalSize > 7 * 1024 * 1024) // 7MB
                    {
                        TempData["Error"] = "Total photo size cannot exceed 7MB";
                        return RedirectToAction("Feedback", new { bookingId });
                    }
                }
                
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    
                    // Insert feedback
                    string query1 = @"INSERT INTO rws_feedback (fdb_bkg_id, fdb_rom_id, fdb_usr_id, fdb_rating, fdb_comment)
                                    VALUES (@bkgId, @romId, @usrId, @rating, @comment);
                                    SELECT CAST(SCOPE_IDENTITY() as int)";
                    
                    int feedbackId;
                    using (SqlCommand cmd = new SqlCommand(query1, conn))
                    {
                        cmd.Parameters.AddWithValue("@bkgId", bookingId);
                        cmd.Parameters.AddWithValue("@romId", roomId);
                        cmd.Parameters.AddWithValue("@usrId", userId);
                        cmd.Parameters.AddWithValue("@rating", rating);
                        cmd.Parameters.AddWithValue("@comment", string.IsNullOrEmpty(comment) ? DBNull.Value : comment);
                        
                        feedbackId = (int)cmd.ExecuteScalar();
                    }
                    
                    // Save photos
                    if (photos != null && photos.Count > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "feedback");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        foreach (var photo in photos)
                        {
                            if (photo.Length > 0)
                            {
                                string uniqueFileName = $"{feedbackId}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                                
                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await photo.CopyToAsync(fileStream);
                                }
                                
                                // Save to database
                                string query2 = @"INSERT INTO rws_feedback_photos (fph_fdb_id, fph_photo_path, fph_photo_size_kb)
                                                VALUES (@fdbId, @path, @size)";
                                
                                using (SqlCommand cmd = new SqlCommand(query2, conn))
                                {
                                    cmd.Parameters.AddWithValue("@fdbId", feedbackId);
                                    cmd.Parameters.AddWithValue("@path", "/uploads/feedback/" + uniqueFileName);
                                    cmd.Parameters.AddWithValue("@size", (int)(photo.Length / 1024));
                                    
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                
                TempData["Success"] = "Feedback submitted successfully";
                return RedirectToAction("MyBookings");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("Feedback", new { bookingId });
            }
        }
        
        // Check Availability (AJAX)
        [HttpGet]
        public JsonResult CheckAvailability(int roomId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            bool isAvailable = !CheckBookingConflict(roomId, date, startTime, endTime, null);
            
            if (!isAvailable)
            {
                // Get conflicting bookings
                List<string> conflicts = new List<string>();
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT bkg_start_time, bkg_end_time, u.usr_name
                                   FROM rws_bookings b
                                   INNER JOIN rws_users u ON b.bkg_usr_id = u.usr_id
                                   WHERE bkg_rom_id = @roomId AND bkg_dt = @date AND bkg_status = 'Upcoming'
                                   AND ((@startTime >= bkg_start_time AND @startTime < bkg_end_time)
                                        OR (@endTime > bkg_start_time AND @endTime <= bkg_end_time)
                                        OR (@startTime <= bkg_start_time AND @endTime >= bkg_end_time))";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@roomId", roomId);
                        cmd.Parameters.AddWithValue("@date", date);
                        cmd.Parameters.AddWithValue("@startTime", startTime);
                        cmd.Parameters.AddWithValue("@endTime", endTime);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TimeSpan st = reader.GetTimeSpan(0);
                                TimeSpan et = reader.GetTimeSpan(1);
                                string user = reader.GetString(2);
                                conflicts.Add($"{st:hh\\:mm} - {et:hh\\:mm} (by {user})");
                            }
                        }
                    }
                }
                
                return Json(new { available = false, conflicts });
            }
            
            return Json(new { available = true });
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
        
        private List<Room> GetAvailableRooms()
        {
            List<Room> rooms = new List<Room>();
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT * FROM rws_rooms WHERE rom_is_active = 1 AND rom_status = 'Available' ORDER BY rom_name";
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
                            rom_has_video_conf = reader.GetBoolean(5)
                        });
                    }
                }
            }
            return rooms;
        }
        
        private byte GetRoomCapacity(int roomId)
        {
            using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
            {
                conn.Open();
                string query = "SELECT rom_capacity FROM rws_rooms WHERE rom_id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", roomId);
                    return (byte)cmd.ExecuteScalar();
                }
            }
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
    }
}
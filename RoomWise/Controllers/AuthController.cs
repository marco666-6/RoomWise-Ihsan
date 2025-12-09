// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RoomWise.Functions;
using RoomWise.Models;

namespace RoomWise.Controllers
{
    public class AuthController : Controller
    {
        private readonly DbAccess _db;
        
        public AuthController()
        {
            _db = new DbAccess();
        }
        
        // GET: Login Page
        public IActionResult Index()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                string? role = HttpContext.Session.GetString("UserRole");
                return role == "Admin" ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Karyawan");
            }
            
            return View();
        }
        
        // POST: Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                string hashedPassword = SecHelper.HashPasswordMD5(password);
                
                using (SqlConnection conn = new SqlConnection(_db.GetConnectionString()))
                {
                    conn.Open();
                    string query = @"SELECT usr_id, usr_name, usr_email, usr_role, usr_dept 
                                   FROM rws_users 
                                   WHERE (usr_email = @username OR usr_name = @username) 
                                   AND usr_password = @password 
                                   AND usr_is_active = 1";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Set session
                                HttpContext.Session.SetInt32("UserId", reader.GetInt32(0));
                                HttpContext.Session.SetString("UserName", reader.GetString(1));
                                HttpContext.Session.SetString("UserEmail", reader.GetString(2));
                                HttpContext.Session.SetString("UserRole", reader.GetString(3));
                                HttpContext.Session.SetString("UserDept", reader.IsDBNull(4) ? "" : reader.GetString(4));
                                
                                // Log audit
                                LogAudit(reader.GetInt32(0), "LOGIN", "USER", reader.GetInt32(0), "User logged in");
                                
                                // Redirect based on role
                                string role = reader.GetString(3);
                                if (role == "Admin")
                                {
                                    return RedirectToAction("Index", "Admin");
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Karyawan");
                                }
                            }
                        }
                    }
                }
                
                TempData["Error"] = "Invalid username or password";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        
        // Logout
        public IActionResult Logout()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                LogAudit(userId.Value, "LOGOUT", "USER", userId.Value, "User logged out");
            }
            
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
        
        // Helper: Log Audit
        private void LogAudit(int userId, string action, string entity, int? entityId, string description)
        {
            try
            {
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
                        cmd.Parameters.AddWithValue("@entityId", entityId ?? (object)DBNull.Value);
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
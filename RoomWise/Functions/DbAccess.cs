using System;

//penghubung antara aplikasi dan database
namespace RoomWise.Functions
{
    public class DbAccess
    {
        private readonly string connectionString;

        //constructor
        public DbAccess()
        {
            // Your SQL Server connection string
            connectionString = @"Data Source=localhost\SQLEXPRESS;Initial Catalog=roomwise_db;Integrated Security=True;TrustServerCertificate=True";
        }

        // Method to get connection string - this is what your controller is looking for
        public string GetConnectionString()
        {
            return connectionString;
        }

    }

}
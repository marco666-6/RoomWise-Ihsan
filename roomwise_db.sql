USE [master]
GO
/****** Object:  Database [roomwise_db]    Script Date: 12/10/2025 4:05:51 PM ******/
CREATE DATABASE [roomwise_db]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'roomwise_db', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\roomwise_db.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'roomwise_db_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQL\DATA\roomwise_db_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [roomwise_db].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [roomwise_db] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [roomwise_db] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [roomwise_db] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [roomwise_db] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [roomwise_db] SET ARITHABORT OFF 
GO
ALTER DATABASE [roomwise_db] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [roomwise_db] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [roomwise_db] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [roomwise_db] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [roomwise_db] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [roomwise_db] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [roomwise_db] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [roomwise_db] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [roomwise_db] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [roomwise_db] SET  ENABLE_BROKER 
GO
ALTER DATABASE [roomwise_db] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [roomwise_db] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [roomwise_db] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [roomwise_db] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [roomwise_db] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [roomwise_db] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [roomwise_db] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [roomwise_db] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [roomwise_db] SET  MULTI_USER 
GO
ALTER DATABASE [roomwise_db] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [roomwise_db] SET DB_CHAINING OFF 
GO
ALTER DATABASE [roomwise_db] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [roomwise_db] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [roomwise_db] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [roomwise_db] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [roomwise_db] SET QUERY_STORE = ON
GO
ALTER DATABASE [roomwise_db] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [roomwise_db]
GO
/****** Object:  Table [dbo].[rws_rooms]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_rooms](
	[rom_id] [int] IDENTITY(1,1) NOT NULL,
	[rom_name] [nvarchar](50) NOT NULL,
	[rom_capacity] [tinyint] NOT NULL,
	[rom_has_projector] [bit] NOT NULL,
	[rom_has_whiteboard] [bit] NOT NULL,
	[rom_has_video_conf] [bit] NOT NULL,
	[rom_status] [varchar](15) NOT NULL,
	[rom_status_reason] [nvarchar](200) NULL,
	[rom_status_updated_dt] [datetime] NULL,
	[rom_status_updated_by] [int] NULL,
	[rom_is_active] [bit] NOT NULL,
	[rom_created_dt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[rom_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[rws_bookings]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_bookings](
	[bkg_id] [int] IDENTITY(1,1) NOT NULL,
	[bkg_rom_id] [int] NOT NULL,
	[bkg_usr_id] [int] NOT NULL,
	[bkg_dt] [date] NOT NULL,
	[bkg_start_time] [time](0) NOT NULL,
	[bkg_end_time] [time](0) NOT NULL,
	[bkg_purpose] [nvarchar](300) NOT NULL,
	[bkg_attendees] [tinyint] NULL,
	[bkg_status] [varchar](10) NOT NULL,
	[bkg_cancelled_dt] [datetime] NULL,
	[bkg_cancelled_reason] [nvarchar](200) NULL,
	[bkg_created_dt] [datetime] NOT NULL,
	[bkg_created_by] [int] NOT NULL,
	[bkg_updated_dt] [datetime] NULL,
	[bkg_updated_by] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[bkg_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_room_availability]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================
-- USEFUL VIEWS
-- ============================================

-- View: Available Rooms with Current Status
CREATE VIEW [dbo].[vw_room_availability] AS
SELECT 
    r.rom_id,
    r.rom_name,
    r.rom_capacity,
    r.rom_has_projector,
    r.rom_has_whiteboard,
    r.rom_has_video_conf,
    r.rom_status,
    r.rom_status_reason,
    CASE 
        WHEN r.rom_status != 'Available' THEN 0
        WHEN EXISTS (
            SELECT 1 FROM rws_bookings b
            WHERE b.bkg_rom_id = r.rom_id
            AND b.bkg_dt = CAST(GETDATE() AS DATE)
            AND CAST(GETDATE() AS TIME) BETWEEN b.bkg_start_time AND b.bkg_end_time
            AND b.bkg_status = 'Upcoming'
        ) THEN 0
        ELSE 1
    END AS rom_is_available_now
FROM rws_rooms r
WHERE r.rom_is_active = 1;
GO
/****** Object:  Table [dbo].[rws_users]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_users](
	[usr_id] [int] IDENTITY(1,1) NOT NULL,
	[usr_name] [nvarchar](100) NOT NULL,
	[usr_email] [varchar](100) NOT NULL,
	[usr_password] [varchar](255) NOT NULL,
	[usr_role] [varchar](10) NOT NULL,
	[usr_dept] [nvarchar](50) NULL,
	[usr_is_active] [bit] NOT NULL,
	[usr_created_dt] [datetime] NOT NULL,
	[usr_updated_dt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[usr_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_todays_bookings]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- View: Today's Bookings
CREATE VIEW [dbo].[vw_todays_bookings] AS
SELECT 
    b.bkg_id,
    r.rom_name,
    u.usr_name AS bkg_pic_name,
    b.bkg_dt,
    b.bkg_start_time,
    b.bkg_end_time,
    b.bkg_purpose,
    b.bkg_attendees,
    b.bkg_status
FROM rws_bookings b
INNER JOIN rws_rooms r ON b.bkg_rom_id = r.rom_id
INNER JOIN rws_users u ON b.bkg_usr_id = u.usr_id
WHERE b.bkg_dt = CAST(GETDATE() AS DATE)
AND b.bkg_status != 'Cancelled';
GO
/****** Object:  Table [dbo].[rws_audit_log]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_audit_log](
	[aud_id] [int] IDENTITY(1,1) NOT NULL,
	[aud_usr_id] [int] NULL,
	[aud_action] [varchar](20) NOT NULL,
	[aud_entity] [varchar](20) NOT NULL,
	[aud_entity_id] [int] NULL,
	[aud_description] [nvarchar](300) NULL,
	[aud_ip_address] [varchar](45) NULL,
	[aud_dt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[aud_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[rws_feedback]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_feedback](
	[fdb_id] [int] IDENTITY(1,1) NOT NULL,
	[fdb_bkg_id] [int] NOT NULL,
	[fdb_rom_id] [int] NOT NULL,
	[fdb_usr_id] [int] NOT NULL,
	[fdb_rating] [tinyint] NOT NULL,
	[fdb_comment] [nvarchar](500) NULL,
	[fdb_created_dt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[fdb_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[rws_feedback_photos]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_feedback_photos](
	[fph_id] [int] IDENTITY(1,1) NOT NULL,
	[fph_fdb_id] [int] NOT NULL,
	[fph_photo_path] [varchar](255) NOT NULL,
	[fph_photo_size_kb] [int] NOT NULL,
	[fph_uploaded_dt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[fph_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[rws_notifications]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_notifications](
	[ntf_id] [int] IDENTITY(1,1) NOT NULL,
	[ntf_usr_id] [int] NOT NULL,
	[ntf_type] [varchar](20) NOT NULL,
	[ntf_title] [nvarchar](100) NOT NULL,
	[ntf_message] [nvarchar](300) NOT NULL,
	[ntf_related_entity] [varchar](20) NULL,
	[ntf_related_id] [int] NULL,
	[ntf_is_read] [bit] NOT NULL,
	[ntf_created_dt] [datetime] NOT NULL,
	[ntf_read_dt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ntf_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[rws_room_status_history]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[rws_room_status_history](
	[rsh_id] [int] IDENTITY(1,1) NOT NULL,
	[rsh_rom_id] [int] NOT NULL,
	[rsh_old_status] [varchar](15) NOT NULL,
	[rsh_new_status] [varchar](15) NOT NULL,
	[rsh_reason] [nvarchar](200) NULL,
	[rsh_changed_by] [int] NOT NULL,
	[rsh_changed_dt] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[rsh_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[rws_audit_log] ON 
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (1, 1, N'LOGIN', N'USER', 1, N'User logged in', N'::1', CAST(N'2025-12-10T12:26:27.607' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (2, 1, N'CREATE', N'ROOM', NULL, N'Created room: Black Room', N'::1', CAST(N'2025-12-10T12:30:01.843' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (3, 1, N'UPDATE', N'ROOM', 6, N'Updated room: Black Room', N'::1', CAST(N'2025-12-10T12:30:11.093' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (4, 1, N'LOGIN', N'USER', 1, N'User logged in', N'::1', CAST(N'2025-12-10T13:06:44.740' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (5, 1, N'CREATE', N'BOOKING', NULL, N'Created booking for user ID 1', N'::1', CAST(N'2025-12-10T13:07:34.010' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (6, 1, N'UPDATE', N'BOOKING', 1, N'Cancelled booking', N'::1', CAST(N'2025-12-10T14:15:47.717' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (7, 1, N'UPDATE', N'ROOM', 6, N'Updated room: Black Room', N'::1', CAST(N'2025-12-10T14:23:05.150' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (8, 1, N'UPDATE', N'ROOM', 3, N'Updated room: Conference Hall', N'::1', CAST(N'2025-12-10T14:23:16.013' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (9, 1, N'UPDATE', N'ROOM', 6, N'Updated room: Black Room', N'::1', CAST(N'2025-12-10T14:32:01.477' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (10, 1, N'UPDATE', N'ROOM', 3, N'Updated room: Conference Hall', N'::1', CAST(N'2025-12-10T14:32:10.957' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (11, 1, N'UPDATE', N'ROOM', 6, N'Changed room status to OutOfService', N'::1', CAST(N'2025-12-10T14:33:56.147' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (12, 1, N'UPDATE', N'USER', 1, N'Updated user: System Admin', N'::1', CAST(N'2025-12-10T14:37:08.247' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (13, 1, N'UPDATE', N'USER', 1, N'Updated user: System Admin', N'::1', CAST(N'2025-12-10T14:37:13.300' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (14, 1, N'UPDATE', N'USER', 1, N'Updated user: System Admin', N'::1', CAST(N'2025-12-10T14:38:06.090' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (15, 1, N'UPDATE', N'USER', 1, N'Updated user: System Admin', N'::1', CAST(N'2025-12-10T14:38:14.310' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (16, 1, N'CREATE', N'USER', NULL, N'Created user: Blacky', N'::1', CAST(N'2025-12-10T14:38:40.970' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (17, 1, N'UPDATE', N'USER', 2, N'Updated user: Blacky', N'::1', CAST(N'2025-12-10T14:38:58.387' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (18, 1, N'UPDATE', N'USER', 2, N'Updated user: Blacky', N'::1', CAST(N'2025-12-10T14:39:25.727' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (19, 2, N'LOGIN', N'USER', 2, N'User logged in', N'::1', CAST(N'2025-12-10T14:39:37.350' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (20, 2, N'UPDATE', N'USER', 2, N'Updated user: Blacky', N'::1', CAST(N'2025-12-10T14:39:48.373' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (21, 2, N'LOGOUT', N'USER', 2, N'User logged out', N'::1', CAST(N'2025-12-10T14:39:49.090' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (22, 1, N'DELETE', N'USER', 2, N'Deactivated user', N'::1', CAST(N'2025-12-10T14:40:02.357' AS DateTime))
GO
INSERT [dbo].[rws_audit_log] ([aud_id], [aud_usr_id], [aud_action], [aud_entity], [aud_entity_id], [aud_description], [aud_ip_address], [aud_dt]) VALUES (23, 1, N'DELETE', N'USER', 2, N'Deactivated user', N'::1', CAST(N'2025-12-10T14:40:10.717' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[rws_audit_log] OFF
GO
SET IDENTITY_INSERT [dbo].[rws_bookings] ON 
GO
INSERT [dbo].[rws_bookings] ([bkg_id], [bkg_rom_id], [bkg_usr_id], [bkg_dt], [bkg_start_time], [bkg_end_time], [bkg_purpose], [bkg_attendees], [bkg_status], [bkg_cancelled_dt], [bkg_cancelled_reason], [bkg_created_dt], [bkg_created_by], [bkg_updated_dt], [bkg_updated_by]) VALUES (1, 4, 1, CAST(N'2025-12-10' AS Date), CAST(N'13:07:00' AS Time), CAST(N'15:07:00' AS Time), N'god things', 10, N'Cancelled', CAST(N'2025-12-10T14:15:47.707' AS DateTime), N'admin''s ded', CAST(N'2025-12-10T13:07:34.000' AS DateTime), 1, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[rws_bookings] OFF
GO
SET IDENTITY_INSERT [dbo].[rws_room_status_history] ON 
GO
INSERT [dbo].[rws_room_status_history] ([rsh_id], [rsh_rom_id], [rsh_old_status], [rsh_new_status], [rsh_reason], [rsh_changed_by], [rsh_changed_dt]) VALUES (1, 6, N'Available', N'OutOfService', N'No more', 1, CAST(N'2025-12-10T14:33:56.140' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[rws_room_status_history] OFF
GO
SET IDENTITY_INSERT [dbo].[rws_rooms] ON 
GO
INSERT [dbo].[rws_rooms] ([rom_id], [rom_name], [rom_capacity], [rom_has_projector], [rom_has_whiteboard], [rom_has_video_conf], [rom_status], [rom_status_reason], [rom_status_updated_dt], [rom_status_updated_by], [rom_is_active], [rom_created_dt]) VALUES (1, N'Meeting Room A', 8, 1, 1, 1, N'Available', NULL, NULL, NULL, 1, CAST(N'2025-12-09T15:28:09.380' AS DateTime))
GO
INSERT [dbo].[rws_rooms] ([rom_id], [rom_name], [rom_capacity], [rom_has_projector], [rom_has_whiteboard], [rom_has_video_conf], [rom_status], [rom_status_reason], [rom_status_updated_dt], [rom_status_updated_by], [rom_is_active], [rom_created_dt]) VALUES (2, N'Meeting Room B', 12, 1, 1, 0, N'Available', NULL, NULL, NULL, 1, CAST(N'2025-12-09T15:28:09.380' AS DateTime))
GO
INSERT [dbo].[rws_rooms] ([rom_id], [rom_name], [rom_capacity], [rom_has_projector], [rom_has_whiteboard], [rom_has_video_conf], [rom_status], [rom_status_reason], [rom_status_updated_dt], [rom_status_updated_by], [rom_is_active], [rom_created_dt]) VALUES (3, N'Conference Hall', 30, 1, 1, 1, N'Available', NULL, NULL, NULL, 1, CAST(N'2025-12-09T15:28:09.380' AS DateTime))
GO
INSERT [dbo].[rws_rooms] ([rom_id], [rom_name], [rom_capacity], [rom_has_projector], [rom_has_whiteboard], [rom_has_video_conf], [rom_status], [rom_status_reason], [rom_status_updated_dt], [rom_status_updated_by], [rom_is_active], [rom_created_dt]) VALUES (4, N'Small Room 1', 4, 0, 1, 0, N'Available', NULL, NULL, NULL, 1, CAST(N'2025-12-09T15:28:09.380' AS DateTime))
GO
INSERT [dbo].[rws_rooms] ([rom_id], [rom_name], [rom_capacity], [rom_has_projector], [rom_has_whiteboard], [rom_has_video_conf], [rom_status], [rom_status_reason], [rom_status_updated_dt], [rom_status_updated_by], [rom_is_active], [rom_created_dt]) VALUES (5, N'Small Room 2', 4, 0, 1, 0, N'Available', NULL, NULL, NULL, 1, CAST(N'2025-12-09T15:28:09.380' AS DateTime))
GO
INSERT [dbo].[rws_rooms] ([rom_id], [rom_name], [rom_capacity], [rom_has_projector], [rom_has_whiteboard], [rom_has_video_conf], [rom_status], [rom_status_reason], [rom_status_updated_dt], [rom_status_updated_by], [rom_is_active], [rom_created_dt]) VALUES (6, N'Black Room', 10, 1, 0, 1, N'OutOfService', N'No more', CAST(N'2025-12-10T14:33:56.130' AS DateTime), 1, 1, CAST(N'2025-12-10T12:30:01.833' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[rws_rooms] OFF
GO
SET IDENTITY_INSERT [dbo].[rws_users] ON 
GO
INSERT [dbo].[rws_users] ([usr_id], [usr_name], [usr_email], [usr_password], [usr_role], [usr_dept], [usr_is_active], [usr_created_dt], [usr_updated_dt]) VALUES (1, N'System Admin', N'admin@ptxyz.com', N'0192023a7bbd73250516f069df18b500', N'Admin', N'IT', 1, CAST(N'2025-12-09T15:28:09.373' AS DateTime), CAST(N'2025-12-10T14:38:14.310' AS DateTime))
GO
INSERT [dbo].[rws_users] ([usr_id], [usr_name], [usr_email], [usr_password], [usr_role], [usr_dept], [usr_is_active], [usr_created_dt], [usr_updated_dt]) VALUES (2, N'Blacky', N'Blacky@Block.com', N'5a240514e9ae1cdec42b5fbc8280d889', N'Admin', N'IT', 0, CAST(N'2025-12-10T14:38:40.963' AS DateTime), CAST(N'2025-12-10T14:39:48.373' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[rws_users] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_audit_action]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_audit_action] ON [dbo].[rws_audit_log]
(
	[aud_action] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_audit_date]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_audit_date] ON [dbo].[rws_audit_log]
(
	[aud_dt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_audit_user]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_audit_user] ON [dbo].[rws_audit_log]
(
	[aud_usr_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_bookings_date]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_bookings_date] ON [dbo].[rws_bookings]
(
	[bkg_dt] ASC,
	[bkg_start_time] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_bookings_room]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_bookings_room] ON [dbo].[rws_bookings]
(
	[bkg_rom_id] ASC,
	[bkg_dt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_bookings_status]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_bookings_status] ON [dbo].[rws_bookings]
(
	[bkg_status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_bookings_user]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_bookings_user] ON [dbo].[rws_bookings]
(
	[bkg_usr_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [UQ__rws_feed__F47436732C9F8052]    Script Date: 12/10/2025 4:05:52 PM ******/
ALTER TABLE [dbo].[rws_feedback] ADD UNIQUE NONCLUSTERED 
(
	[fdb_bkg_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_feedback_rating]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_feedback_rating] ON [dbo].[rws_feedback]
(
	[fdb_rating] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_feedback_room]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_feedback_room] ON [dbo].[rws_feedback]
(
	[fdb_rom_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_feedback_photos]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_feedback_photos] ON [dbo].[rws_feedback_photos]
(
	[fph_fdb_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_notifications_date]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_notifications_date] ON [dbo].[rws_notifications]
(
	[ntf_created_dt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_notifications_user]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_notifications_user] ON [dbo].[rws_notifications]
(
	[ntf_usr_id] ASC,
	[ntf_is_read] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_room_history_date]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_room_history_date] ON [dbo].[rws_room_status_history]
(
	[rsh_changed_dt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_room_history_room]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_room_history_room] ON [dbo].[rws_room_status_history]
(
	[rsh_rom_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__rws_room__B4535D6AC754AEB6]    Script Date: 12/10/2025 4:05:52 PM ******/
ALTER TABLE [dbo].[rws_rooms] ADD UNIQUE NONCLUSTERED 
(
	[rom_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_rooms_capacity]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_rooms_capacity] ON [dbo].[rws_rooms]
(
	[rom_capacity] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_rooms_status]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_rooms_status] ON [dbo].[rws_rooms]
(
	[rom_status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__rws_user__0F570E77D28B424B]    Script Date: 12/10/2025 4:05:52 PM ******/
ALTER TABLE [dbo].[rws_users] ADD UNIQUE NONCLUSTERED 
(
	[usr_email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [idx_users_active]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_users_active] ON [dbo].[rws_users]
(
	[usr_is_active] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_users_email]    Script Date: 12/10/2025 4:05:52 PM ******/
CREATE NONCLUSTERED INDEX [idx_users_email] ON [dbo].[rws_users]
(
	[usr_email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[rws_audit_log] ADD  DEFAULT (getdate()) FOR [aud_dt]
GO
ALTER TABLE [dbo].[rws_bookings] ADD  DEFAULT ('Upcoming') FOR [bkg_status]
GO
ALTER TABLE [dbo].[rws_bookings] ADD  DEFAULT (getdate()) FOR [bkg_created_dt]
GO
ALTER TABLE [dbo].[rws_feedback] ADD  DEFAULT (getdate()) FOR [fdb_created_dt]
GO
ALTER TABLE [dbo].[rws_feedback_photos] ADD  DEFAULT (getdate()) FOR [fph_uploaded_dt]
GO
ALTER TABLE [dbo].[rws_notifications] ADD  DEFAULT ((0)) FOR [ntf_is_read]
GO
ALTER TABLE [dbo].[rws_notifications] ADD  DEFAULT (getdate()) FOR [ntf_created_dt]
GO
ALTER TABLE [dbo].[rws_room_status_history] ADD  DEFAULT (getdate()) FOR [rsh_changed_dt]
GO
ALTER TABLE [dbo].[rws_rooms] ADD  DEFAULT ((0)) FOR [rom_has_projector]
GO
ALTER TABLE [dbo].[rws_rooms] ADD  DEFAULT ((0)) FOR [rom_has_whiteboard]
GO
ALTER TABLE [dbo].[rws_rooms] ADD  DEFAULT ((0)) FOR [rom_has_video_conf]
GO
ALTER TABLE [dbo].[rws_rooms] ADD  DEFAULT ('Available') FOR [rom_status]
GO
ALTER TABLE [dbo].[rws_rooms] ADD  DEFAULT ((1)) FOR [rom_is_active]
GO
ALTER TABLE [dbo].[rws_rooms] ADD  DEFAULT (getdate()) FOR [rom_created_dt]
GO
ALTER TABLE [dbo].[rws_users] ADD  DEFAULT ((1)) FOR [usr_is_active]
GO
ALTER TABLE [dbo].[rws_users] ADD  DEFAULT (getdate()) FOR [usr_created_dt]
GO
ALTER TABLE [dbo].[rws_bookings]  WITH CHECK ADD FOREIGN KEY([bkg_created_by])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_bookings]  WITH CHECK ADD FOREIGN KEY([bkg_rom_id])
REFERENCES [dbo].[rws_rooms] ([rom_id])
GO
ALTER TABLE [dbo].[rws_bookings]  WITH CHECK ADD FOREIGN KEY([bkg_usr_id])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_bookings]  WITH CHECK ADD FOREIGN KEY([bkg_updated_by])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_feedback]  WITH CHECK ADD FOREIGN KEY([fdb_bkg_id])
REFERENCES [dbo].[rws_bookings] ([bkg_id])
GO
ALTER TABLE [dbo].[rws_feedback]  WITH CHECK ADD FOREIGN KEY([fdb_rom_id])
REFERENCES [dbo].[rws_rooms] ([rom_id])
GO
ALTER TABLE [dbo].[rws_feedback]  WITH CHECK ADD FOREIGN KEY([fdb_usr_id])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_feedback_photos]  WITH CHECK ADD FOREIGN KEY([fph_fdb_id])
REFERENCES [dbo].[rws_feedback] ([fdb_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[rws_notifications]  WITH CHECK ADD FOREIGN KEY([ntf_usr_id])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_room_status_history]  WITH CHECK ADD FOREIGN KEY([rsh_changed_by])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_room_status_history]  WITH CHECK ADD FOREIGN KEY([rsh_rom_id])
REFERENCES [dbo].[rws_rooms] ([rom_id])
GO
ALTER TABLE [dbo].[rws_rooms]  WITH CHECK ADD FOREIGN KEY([rom_status_updated_by])
REFERENCES [dbo].[rws_users] ([usr_id])
GO
ALTER TABLE [dbo].[rws_bookings]  WITH CHECK ADD CHECK  (([bkg_status]='Cancelled' OR [bkg_status]='Completed' OR [bkg_status]='Upcoming'))
GO
ALTER TABLE [dbo].[rws_feedback]  WITH CHECK ADD CHECK  (([fdb_rating]>=(1) AND [fdb_rating]<=(5)))
GO
ALTER TABLE [dbo].[rws_rooms]  WITH CHECK ADD CHECK  (([rom_status]='NotReady' OR [rom_status]='OutOfService' OR [rom_status]='Maintenance' OR [rom_status]='Available'))
GO
ALTER TABLE [dbo].[rws_users]  WITH CHECK ADD CHECK  (([usr_role]='Karyawan' OR [usr_role]='Admin'))
GO
/****** Object:  StoredProcedure [dbo].[sp_check_room_availability]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================
-- STORED PROCEDURES
-- ============================================

-- Check Room Availability
CREATE PROCEDURE [dbo].[sp_check_room_availability]
    @rom_id INT,
    @bkg_dt DATE,
    @start_time TIME,
    @end_time TIME,
    @exclude_bkg_id INT = NULL
AS
BEGIN
    SELECT COUNT(*) AS conflict_count
    FROM rws_bookings
    WHERE bkg_rom_id = @rom_id
    AND bkg_dt = @bkg_dt
    AND bkg_status = 'Upcoming'
    AND (@exclude_bkg_id IS NULL OR bkg_id != @exclude_bkg_id)
    AND (
        (@start_time >= bkg_start_time AND @start_time < bkg_end_time)
        OR (@end_time > bkg_start_time AND @end_time <= bkg_end_time)
        OR (@start_time <= bkg_start_time AND @end_time >= bkg_end_time)
    );
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_get_room_statistics]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get Room Statistics
CREATE PROCEDURE [dbo].[sp_get_room_statistics]
    @rom_id INT = NULL,
    @start_dt DATE = NULL,
    @end_dt DATE = NULL
AS
BEGIN
    SELECT 
        r.rom_id,
        r.rom_name,
        COUNT(b.bkg_id) AS total_bookings,
        AVG(CAST(f.fdb_rating AS FLOAT)) AS avg_rating,
        SUM(DATEDIFF(MINUTE, b.bkg_start_time, b.bkg_end_time)) AS total_usage_min
    FROM rws_rooms r
    LEFT JOIN rws_bookings b ON r.rom_id = b.bkg_rom_id
        AND b.bkg_status = 'Completed'
        AND (@start_dt IS NULL OR b.bkg_dt >= @start_dt)
        AND (@end_dt IS NULL OR b.bkg_dt <= @end_dt)
    LEFT JOIN rws_feedback f ON b.bkg_id = f.fdb_bkg_id
    WHERE r.rom_is_active = 1
    AND (@rom_id IS NULL OR r.rom_id = @rom_id)
    GROUP BY r.rom_id, r.rom_name;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_get_user_bookings]    Script Date: 12/10/2025 4:05:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Get User Booking History
CREATE PROCEDURE [dbo].[sp_get_user_bookings]
    @usr_id INT,
    @start_dt DATE = NULL,
    @end_dt DATE = NULL,
    @status VARCHAR(10) = NULL
AS
BEGIN
    SELECT 
        b.bkg_id,
        r.rom_name,
        b.bkg_dt,
        b.bkg_start_time,
        b.bkg_end_time,
        b.bkg_purpose,
        b.bkg_attendees,
        b.bkg_status,
        DATEDIFF(MINUTE, b.bkg_start_time, b.bkg_end_time) AS bkg_duration_min
    FROM rws_bookings b
    INNER JOIN rws_rooms r ON b.bkg_rom_id = r.rom_id
    WHERE b.bkg_usr_id = @usr_id
    AND (@start_dt IS NULL OR b.bkg_dt >= @start_dt)
    AND (@end_dt IS NULL OR b.bkg_dt <= @end_dt)
    AND (@status IS NULL OR b.bkg_status = @status)
    ORDER BY b.bkg_dt DESC, b.bkg_start_time DESC;
END;
GO
USE [master]
GO
ALTER DATABASE [roomwise_db] SET  READ_WRITE 
GO

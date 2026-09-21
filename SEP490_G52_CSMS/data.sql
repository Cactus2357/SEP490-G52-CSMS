/*******************************************************************************
   CSMS (Coffee Shop Management System) - Database Export
   Database : CSMSSystem
   Generated: 2026-09-21 15:00:17
   Schema & Data Full Export
*******************************************************************************/
USE [master];
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CSMSSystem')
BEGIN
    CREATE DATABASE [CSMSSystem];
END
GO
USE [CSMSSystem];
GO

-- Disable constraints to guarantee clean import
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all';
GO

/*******************************************************************************
   PART 1: TABLES AND PRIMARY KEYS SCHEMA
*******************************************************************************/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[ProductVersion] [nvarchar](32) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[attendance_logs](
	[attendance_id] [int] IDENTITY(1,1) NOT NULL,
	[roster_id] [int] NOT NULL,
	[employee_id] [int] NOT NULL,
	[check_in_time] [datetime2](7) NULL,
	[is_face_check_in_valid] [bit] NOT NULL,
	[check_in_confidence] [decimal](18, 2) NULL,
	[check_in_status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[check_out_time] [datetime2](7) NULL,
	[is_face_check_out_valid] [bit] NOT NULL,
	[check_out_confidence] [decimal](18, 2) NULL,
	[check_out_status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[overall_status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[notes] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_attendance_logs] PRIMARY KEY CLUSTERED 
(
	[attendance_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_attendance_logs_employee_id] ON [dbo].[attendance_logs]
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_attendance_logs_roster_id_employee_id] ON [dbo].[attendance_logs]
(
	[roster_id] ASC,
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branch_inventories](
	[inventory_id] [int] IDENTITY(1,1) NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[material_id] [int] NOT NULL,
	[stock_quantity] [decimal](18, 2) NOT NULL,
	[low_stock_threshold] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_branch_inventories] PRIMARY KEY CLUSTERED 
(
	[inventory_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_branch_inventories_branch_id_material_id] ON [dbo].[branch_inventories]
(
	[branch_id] ASC,
	[material_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_branch_inventories_material_id] ON [dbo].[branch_inventories]
(
	[material_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branch_managers](
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[manager_id] [int] NOT NULL,
	[appointed_date] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_branch_managers] PRIMARY KEY CLUSTERED 
(
	[branch_id] ASC,
	[manager_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_branch_managers_manager_id] ON [dbo].[branch_managers]
(
	[manager_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branch_menus](
	[menu_id] [int] IDENTITY(1,1) NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[menu_name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[is_active] [bit] NOT NULL,
	[updated_at] [datetime2](7) NULL,
 CONSTRAINT [PK_branch_menus] PRIMARY KEY CLUSTERED 
(
	[menu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_branch_menus_branch_id] ON [dbo].[branch_menus]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branch_settings](
	[setting_id] [int] IDENTITY(1,1) NOT NULL,
	[branch_id] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[branch_display_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[contact_phone] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[address] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[opening_hours] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[auto_print_receipt] [bit] NOT NULL,
	[enable_sound_notification] [bit] NOT NULL,
	[bank_code] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[account_number] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[account_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[sepay_api_key] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[webhook_secret_token] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[transfer_prefix] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[auto_confirm_order] [bit] NOT NULL,
	[is_sepay_active] [bit] NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_branch_settings] PRIMARY KEY CLUSTERED 
(
	[setting_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('CSMS Coffee') FOR [branch_display_name]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('0988888888') FOR [contact_phone]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('') FOR [address]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('06:30 - 22:30') FOR [opening_hours]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ((1)) FOR [auto_print_receipt]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ((1)) FOR [enable_sound_notification]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('MBBank') FOR [bank_code]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('0333333333') FOR [account_number]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('CSMS CAFE') FOR [account_name]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('') FOR [sepay_api_key]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('') FOR [webhook_secret_token]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ('CSMS') FOR [transfer_prefix]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ((1)) FOR [auto_confirm_order]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT ((1)) FOR [is_sepay_active]
GO
ALTER TABLE [dbo].[branch_settings] ADD  DEFAULT (getdate()) FOR [updated_at]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branch_supply_request_items](
	[request_item_id] [int] IDENTITY(1,1) NOT NULL,
	[request_id] [int] NOT NULL,
	[material_id] [int] NOT NULL,
	[quantity_requested] [decimal](18, 2) NOT NULL,
	[quantity_released] [decimal](18, 2) NULL,
	[quantity_received] [decimal](18, 2) NULL,
	[quantity_accepted] [decimal](18, 2) NULL,
	[quantity_defective] [decimal](18, 2) NULL,
	[defect_type] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[defect_note] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[defect_image_url] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_branch_supply_request_items] PRIMARY KEY CLUSTERED 
(
	[request_item_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_branch_supply_request_items_material_id] ON [dbo].[branch_supply_request_items]
(
	[material_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_branch_supply_request_items_request_id] ON [dbo].[branch_supply_request_items]
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branch_supply_requests](
	[request_id] [int] IDENTITY(1,1) NOT NULL,
	[request_code] [nvarchar](15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[request_date] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[warehouse_note] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[approved_by] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[approved_date] [datetime2](7) NULL,
	[deliverer_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[deliverer_phone] [nvarchar](15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[received_date] [datetime2](7) NULL,
	[delivery_provider] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[expected_delivery_date] [datetime2](7) NULL,
	[receiver_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[receiver_phone] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[request_note] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[inspected_by] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[inspected_at] [datetime2](7) NULL,
	[inspection_status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_branch_supply_requests] PRIMARY KEY CLUSTERED 
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_branch_supply_requests_branch_id] ON [dbo].[branch_supply_requests]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[branches](
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[branch_name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[address] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[phone_number] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[email] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[opening_time] [time](7) NOT NULL,
	[closing_time] [time](7) NOT NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_branches] PRIMARY KEY CLUSTERED 
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_branches_branch_name] ON [dbo].[branches]
(
	[branch_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cash_handovers](
	[handover_id] [int] IDENTITY(1,1) NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[handover_date] [datetime2](7) NOT NULL,
	[shift_id] [int] NOT NULL,
	[outgoing_cashier_id] [int] NOT NULL,
	[incoming_cashier_id] [int] NOT NULL,
	[initial_cash] [decimal](18, 2) NOT NULL,
	[machine_cash_revenue] [decimal](18, 2) NOT NULL,
	[bank_transfer_revenue] [decimal](18, 2) NOT NULL,
	[theoretical_cash] [decimal](18, 2) NOT NULL,
	[actual_cash] [decimal](18, 2) NOT NULL,
	[notes] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[is_password_confirmed] [bit] NOT NULL,
	[status] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[opened_at] [datetime2](7) NOT NULL,
	[closed_at] [datetime2](7) NULL,
	[deliverer_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[cash_refund_amount] [decimal](18, 2) NOT NULL,
	[handover_type] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[emergency_reason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[retained_cash] [decimal](18, 2) NOT NULL,
	[deposited_cash] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_cash_handovers] PRIMARY KEY CLUSTERED 
(
	[handover_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_cash_handovers_branch_id] ON [dbo].[cash_handovers]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_cash_handovers_incoming_cashier_id] ON [dbo].[cash_handovers]
(
	[incoming_cashier_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_cash_handovers_outgoing_cashier_id] ON [dbo].[cash_handovers]
(
	[outgoing_cashier_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_cash_handovers_shift_id] ON [dbo].[cash_handovers]
(
	[shift_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[cash_handovers] ADD  DEFAULT ((0)) FOR [cash_refund_amount]
GO
ALTER TABLE [dbo].[cash_handovers] ADD  DEFAULT ('Normal') FOR [handover_type]
GO
ALTER TABLE [dbo].[cash_handovers] ADD  DEFAULT ((0)) FOR [retained_cash]
GO
ALTER TABLE [dbo].[cash_handovers] ADD  DEFAULT ((0)) FOR [deposited_cash]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[employees](
	[employee_id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[password] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[full_name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[date_of_birth] [datetime2](7) NOT NULL,
	[address] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[phone_number] [nvarchar](15) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[email] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[role] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[employment_type] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[citizen_id] [nvarchar](12) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[contract_file_path] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[cccd_file_path] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[status] [nvarchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[face_data] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[failed_login_attempts] [int] NOT NULL,
	[lockout_until] [datetime2](7) NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_employees] PRIMARY KEY CLUSTERED 
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_employees_branch_id] ON [dbo].[employees]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_employees_citizen_id] ON [dbo].[employees]
(
	[citizen_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_employees_email] ON [dbo].[employees]
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_employees_username] ON [dbo].[employees]
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[fixed_shifts](
	[shift_id] [int] IDENTITY(1,1) NOT NULL,
	[shift_name] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[start_time] [time](7) NOT NULL,
	[end_time] [time](7) NOT NULL,
 CONSTRAINT [PK_fixed_shifts] PRIMARY KEY CLUSTERED 
(
	[shift_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[leave_applications](
	[application_id] [int] IDENTITY(1,1) NOT NULL,
	[employee_id] [int] NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[reason] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[leave_shifts] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[submitted_at] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[approved_branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[approved_manager_id] [int] NULL,
 CONSTRAINT [PK_leave_applications] PRIMARY KEY CLUSTERED 
(
	[application_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_leave_applications_approved_branch_id_approved_manager_id] ON [dbo].[leave_applications]
(
	[approved_branch_id] ASC,
	[approved_manager_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_leave_applications_employee_id] ON [dbo].[leave_applications]
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[master_products](
	[product_id] [int] IDENTITY(1,1) NOT NULL,
	[product_name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[image_url] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[description] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[category_id] [int] NOT NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_master_products] PRIMARY KEY CLUSTERED 
(
	[product_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_master_products_category_id] ON [dbo].[master_products]
(
	[category_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_master_products_product_name] ON [dbo].[master_products]
(
	[product_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[material_categories](
	[category_id] [int] IDENTITY(1,1) NOT NULL,
	[category_name] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[description] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_material_categories] PRIMARY KEY CLUSTERED 
(
	[category_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_material_categories_category_name] ON [dbo].[material_categories]
(
	[category_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[materials](
	[material_id] [int] IDENTITY(1,1) NOT NULL,
	[material_code] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[material_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[material_kind] [nvarchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[category] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[supplier] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[unit_price] [decimal](18, 2) NOT NULL,
	[origin] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[storage_unit] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[physical_state] [nvarchar](30) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[stock_quantity] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_materials] PRIMARY KEY CLUSTERED 
(
	[material_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_materials_material_name_supplier] ON [dbo].[materials]
(
	[material_name] ASC,
	[supplier] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[materials] ADD  DEFAULT (N'') FOR [physical_state]
GO
ALTER TABLE [dbo].[materials] ADD  DEFAULT ((0.0)) FOR [stock_quantity]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[menu_details](
	[menu_id] [int] NOT NULL,
	[variant_id] [int] NOT NULL,
	[is_available] [bit] NOT NULL,
	[updated_by] [int] NULL,
	[updated_at] [datetime2](7) NULL,
 CONSTRAINT [PK_menu_details] PRIMARY KEY CLUSTERED 
(
	[menu_id] ASC,
	[variant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_menu_details_variant_id] ON [dbo].[menu_details]
(
	[variant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[menu_details] ADD  DEFAULT ((1)) FOR [is_available]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[notifications](
	[notification_id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[message] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[created_time] [datetime2](7) NOT NULL,
	[is_read] [bit] NOT NULL,
	[recipient_user_id] [int] NULL,
	[recipient_role] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[resource_url] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[branch_id] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_notifications] PRIMARY KEY CLUSTERED 
(
	[notification_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[order_items](
	[order_id] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[variant_id] [int] NOT NULL,
	[quantity] [int] NOT NULL,
	[unit_price] [decimal](18, 2) NOT NULL,
	[is_completed] [bit] NOT NULL,
 CONSTRAINT [PK_order_items] PRIMARY KEY CLUSTERED 
(
	[order_id] ASC,
	[variant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_order_items_variant_id] ON [dbo].[order_items]
(
	[variant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[order_items] ADD  CONSTRAINT [DF_order_items_is_completed]  DEFAULT ((0)) FOR [is_completed]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[orders](
	[order_id] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[cashier_id] [int] NOT NULL,
	[created_at] [datetime2](7) NOT NULL,
	[total_amount] [decimal](18, 2) NOT NULL,
	[payment_method] [nvarchar](200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[payment_status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[brewing_status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[recipient_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[table_number] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[customer_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[subtotal_amount] [decimal](18, 2) NOT NULL,
	[discount_amount] [decimal](18, 2) NOT NULL,
	[trade_discount_amount] [decimal](18, 2) NOT NULL,
	[order_notes] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[bank_transaction_code] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[refund_amount] [decimal](18, 2) NOT NULL,
	[refund_reason] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[refund_method] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[refunded_at] [datetime2](7) NULL,
	[cash_amount] [decimal](18, 2) NOT NULL,
	[bank_amount] [decimal](18, 2) NOT NULL,
	[voucher_id] [int] NULL,
	[voucher_code] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_orders] PRIMARY KEY CLUSTERED 
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_orders_branch_id] ON [dbo].[orders]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_orders_cashier_id] ON [dbo].[orders]
(
	[cashier_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [subtotal_amount]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [discount_amount]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [trade_discount_amount]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [refund_amount]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [cash_amount]
GO
ALTER TABLE [dbo].[orders] ADD  DEFAULT ((0)) FOR [bank_amount]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[payments](
	[payment_id] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[order_id] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[cashier_id] [int] NULL,
	[payment_type] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[payment_method] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[amount] [decimal](18, 2) NOT NULL,
	[status] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[transaction_code] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[customer_cash] [decimal](18, 2) NULL,
	[change_amount] [decimal](18, 2) NULL,
	[notes] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[created_at] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_payments] PRIMARY KEY CLUSTERED 
(
	[payment_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_payments_branch_id] ON [dbo].[payments]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_payments_created_at] ON [dbo].[payments]
(
	[created_at] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_payments_order_id] ON [dbo].[payments]
(
	[order_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[payments] ADD  CONSTRAINT [DF_payments_payment_type]  DEFAULT ('Payment') FOR [payment_type]
GO
ALTER TABLE [dbo].[payments] ADD  CONSTRAINT [DF_payments_status]  DEFAULT ('Success') FOR [status]
GO
ALTER TABLE [dbo].[payments] ADD  CONSTRAINT [DF_payments_created_at]  DEFAULT (getdate()) FOR [created_at]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product_categories](
	[category_id] [int] IDENTITY(1,1) NOT NULL,
	[category_name] [nvarchar](150) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[description] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_product_categories] PRIMARY KEY CLUSTERED 
(
	[category_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_product_categories_category_name] ON [dbo].[product_categories]
(
	[category_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[product_variants](
	[variant_id] [int] IDENTITY(1,1) NOT NULL,
	[product_id] [int] NOT NULL,
	[size_variant] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[selling_price] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_product_variants] PRIMARY KEY CLUSTERED 
(
	[variant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_product_variants_product_id_size_variant] ON [dbo].[product_variants]
(
	[product_id] ASC,
	[size_variant] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[recipes](
	[recipe_id] [int] IDENTITY(1,1) NOT NULL,
	[variant_id] [int] NOT NULL,
	[material_id] [int] NOT NULL,
	[quantity] [decimal](18, 2) NOT NULL,
	[branch_id] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
 CONSTRAINT [PK_recipes] PRIMARY KEY CLUSTERED 
(
	[recipe_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_recipes_material_id] ON [dbo].[recipes]
(
	[material_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_recipes_variant_id] ON [dbo].[recipes]
(
	[variant_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[shift_change_requests](
	[request_id] [int] IDENTITY(1,1) NOT NULL,
	[requesting_employee_id] [int] NOT NULL,
	[aspiration] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[reason] [nvarchar](max) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[submitted_at] [datetime2](7) NOT NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[approved_branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[approved_manager_id] [int] NULL,
 CONSTRAINT [PK_shift_change_requests] PRIMARY KEY CLUSTERED 
(
	[request_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_shift_change_requests_approved_branch_id_approved_manager_id] ON [dbo].[shift_change_requests]
(
	[approved_branch_id] ASC,
	[approved_manager_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_shift_change_requests_requesting_employee_id] ON [dbo].[shift_change_requests]
(
	[requesting_employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[vouchers](
	[voucher_id] [int] IDENTITY(1,1) NOT NULL,
	[voucher_code] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[discount_percent] [decimal](5, 2) NOT NULL,
	[quantity] [int] NOT NULL,
	[used_count] [int] NOT NULL,
	[start_date] [datetime2](7) NOT NULL,
	[end_date] [datetime2](7) NOT NULL,
	[description] [nvarchar](500) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[is_active] [bit] NOT NULL,
	[created_by] [int] NULL,
	[created_at] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_vouchers] PRIMARY KEY CLUSTERED 
(
	[voucher_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_vouchers_branch_id] ON [dbo].[vouchers]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_vouchers_dates] ON [dbo].[vouchers]
(
	[start_date] ASC,
	[end_date] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_vouchers_voucher_code] ON [dbo].[vouchers]
(
	[voucher_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[vouchers] ADD  CONSTRAINT [DF_vouchers_used_count]  DEFAULT ((0)) FOR [used_count]
GO
ALTER TABLE [dbo].[vouchers] ADD  CONSTRAINT [DF_vouchers_is_active]  DEFAULT ((1)) FOR [is_active]
GO
ALTER TABLE [dbo].[vouchers] ADD  CONSTRAINT [DF_vouchers_created_at]  DEFAULT (getdate()) FOR [created_at]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[warehouse_receipt_items](
	[receipt_item_id] [int] IDENTITY(1,1) NOT NULL,
	[receipt_id] [int] NOT NULL,
	[material_id] [int] NOT NULL,
	[quantity] [decimal](18, 2) NOT NULL,
	[unit_price] [decimal](18, 2) NOT NULL,
	[amount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_warehouse_receipt_items] PRIMARY KEY CLUSTERED 
(
	[receipt_item_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_warehouse_receipt_items_material_id] ON [dbo].[warehouse_receipt_items]
(
	[material_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_warehouse_receipt_items_receipt_id] ON [dbo].[warehouse_receipt_items]
(
	[receipt_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[warehouse_receipts](
	[receipt_id] [int] IDENTITY(1,1) NOT NULL,
	[receipt_code] [nvarchar](10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[import_date] [datetime2](7) NOT NULL,
	[supplier] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[total_amount] [decimal](18, 2) NOT NULL,
	[status] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[created_by] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[deliverer_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[deliverer_phone] [nvarchar](15) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[receiver_name] [nvarchar](100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
 CONSTRAINT [PK_warehouse_receipts] PRIMARY KEY CLUSTERED 
(
	[receipt_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_warehouse_receipts_receipt_code] ON [dbo].[warehouse_receipts]
(
	[receipt_code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[warehouse_receipts] ADD  DEFAULT (N'') FOR [created_by]
GO
ALTER TABLE [dbo].[warehouse_receipts] ADD  DEFAULT (N'') FOR [deliverer_name]
GO
ALTER TABLE [dbo].[warehouse_receipts] ADD  DEFAULT (N'') FOR [receiver_name]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[weekly_roster_grids](
	[roster_id] [int] IDENTITY(1,1) NOT NULL,
	[branch_id] [nvarchar](20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[assignment_date] [datetime2](7) NOT NULL,
	[shift_id] [int] NOT NULL,
	[employee_id] [int] NOT NULL,
 CONSTRAINT [PK_weekly_roster_grids] PRIMARY KEY CLUSTERED 
(
	[roster_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_weekly_roster_grids_branch_id] ON [dbo].[weekly_roster_grids]
(
	[branch_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_weekly_roster_grids_employee_id] ON [dbo].[weekly_roster_grids]
(
	[employee_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_weekly_roster_grids_shift_id] ON [dbo].[weekly_roster_grids]
(
	[shift_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

/*******************************************************************************
   PART 2: DATA INSERT STATEMENTS
*******************************************************************************/
-- -------------------------------------------------------------
-- Data for table: __EFMigrationsHistory (10 rows)
-- -------------------------------------------------------------
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260730091342_InitialCreate', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260807060053_CashHandoverDetails', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260808130440_AddMaterialsAndRecipes', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260808134330_UpdateMaterialsForWarehouse', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260808184407_AddWarehouseCategoriesAndReceipts', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260808214724_AddDetailsToWarehouseReceiptsAndAddBranchInventory', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260809042708_AddBranchSupplyRequests', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260809044955_AddNotificationsTable', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260809085209_AddNotificationResourceUrl', N'8.0.0')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20260815130125_BranchSeparateNotification', N'8.0.0')
GO

-- -------------------------------------------------------------
-- Data for table: attendance_logs (86 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[attendance_logs] ON
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (1, 1, 6, CAST(N'2026-07-20T08:46:00.0000000' AS DateTime2), 1, CAST(95.65 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T15:56:00.0000000' AS DateTime2), 1, CAST(96.49 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2, 2, 3, CAST(N'2026-07-20T07:57:00.0000000' AS DateTime2), 1, CAST(94.29 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T21:01:00.0000000' AS DateTime2), 1, CAST(93.39 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3, 3, 5, NULL, 0, NULL, N'Absent', NULL, 0, NULL, N'NotYetCheckOut', N'Absent', N'Nghỉ không phép')
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (4, 4, 7, CAST(N'2026-07-20T08:50:00.0000000' AS DateTime2), 1, CAST(91.46 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T19:26:00.0000000' AS DateTime2), 1, CAST(90.67 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (5, 5, 4, CAST(N'2026-07-21T06:30:00.0000000' AS DateTime2), 1, CAST(98.91 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-21T20:32:00.0000000' AS DateTime2), 1, CAST(91.80 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (6, 6, 6, NULL, 0, NULL, N'Absent', NULL, 0, NULL, N'NotYetCheckOut', N'Absent', N'Nghỉ không phép')
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (7, 7, 3, CAST(N'2026-07-21T08:00:00.0000000' AS DateTime2), 1, CAST(94.79 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-21T20:15:00.0000000' AS DateTime2), 1, CAST(90.04 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (8, 8, 8, CAST(N'2026-07-21T08:51:00.0000000' AS DateTime2), 1, CAST(95.45 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-21T16:14:00.0000000' AS DateTime2), 1, CAST(95.87 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (9, 9, 6, CAST(N'2026-07-22T06:51:00.0000000' AS DateTime2), 1, CAST(91.79 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T21:45:00.0000000' AS DateTime2), 1, CAST(98.13 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (10, 10, 5, CAST(N'2026-07-22T06:20:00.0000000' AS DateTime2), 1, CAST(92.86 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T15:24:00.0000000' AS DateTime2), 1, CAST(96.19 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (11, 11, 8, CAST(N'2026-07-22T06:08:00.0000000' AS DateTime2), 1, CAST(91.61 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T19:27:00.0000000' AS DateTime2), 1, CAST(92.05 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (12, 12, 7, CAST(N'2026-07-22T08:29:00.0000000' AS DateTime2), 1, CAST(95.16 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T20:42:00.0000000' AS DateTime2), 1, CAST(91.02 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (13, 13, 4, CAST(N'2026-07-23T06:04:00.0000000' AS DateTime2), 1, CAST(91.90 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T20:52:00.0000000' AS DateTime2), 1, CAST(92.47 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (14, 14, 7, CAST(N'2026-07-23T06:36:00.0000000' AS DateTime2), 1, CAST(95.58 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T18:09:00.0000000' AS DateTime2), 1, CAST(90.11 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (15, 15, 3, CAST(N'2026-07-23T07:12:00.0000000' AS DateTime2), 1, CAST(96.00 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T20:19:00.0000000' AS DateTime2), 1, CAST(96.82 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (16, 16, 8, CAST(N'2026-07-23T08:25:00.0000000' AS DateTime2), 1, CAST(94.87 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T20:25:00.0000000' AS DateTime2), 1, CAST(91.59 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (17, 17, 4, CAST(N'2026-07-24T07:37:00.0000000' AS DateTime2), 1, CAST(91.44 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T15:47:00.0000000' AS DateTime2), 1, CAST(96.79 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (18, 18, 5, CAST(N'2026-07-24T06:06:00.0000000' AS DateTime2), 1, CAST(92.67 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T18:24:00.0000000' AS DateTime2), 1, CAST(96.53 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (19, 19, 8, CAST(N'2026-07-24T07:33:00.0000000' AS DateTime2), 1, CAST(92.32 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T16:12:00.0000000' AS DateTime2), 1, CAST(98.32 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (20, 20, 6, CAST(N'2026-07-24T06:54:00.0000000' AS DateTime2), 1, CAST(98.20 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T17:21:00.0000000' AS DateTime2), 1, CAST(91.95 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (21, 41, 14, CAST(N'2026-07-20T07:40:00.0000000' AS DateTime2), 1, CAST(90.51 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T18:05:00.0000000' AS DateTime2), 1, CAST(91.41 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (22, 42, 10, CAST(N'2026-07-20T08:27:00.0000000' AS DateTime2), 1, CAST(93.49 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T17:15:00.0000000' AS DateTime2), 1, CAST(98.22 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (23, 43, 11, CAST(N'2026-07-20T06:22:00.0000000' AS DateTime2), 1, CAST(92.98 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T21:01:00.0000000' AS DateTime2), 1, CAST(96.95 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (24, 44, 12, CAST(N'2026-07-20T08:17:00.0000000' AS DateTime2), 1, CAST(94.50 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-20T16:15:00.0000000' AS DateTime2), 1, CAST(90.32 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (25, 45, 10, NULL, 0, NULL, N'Absent', NULL, 0, NULL, N'NotYetCheckOut', N'Absent', N'Nghỉ không phép')
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (26, 46, 15, CAST(N'2026-07-21T06:38:00.0000000' AS DateTime2), 1, CAST(93.90 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-21T18:12:00.0000000' AS DateTime2), 1, CAST(95.52 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (27, 47, 12, CAST(N'2026-07-21T06:03:00.0000000' AS DateTime2), 1, CAST(92.85 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-21T18:11:00.0000000' AS DateTime2), 1, CAST(95.05 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (28, 48, 14, NULL, 0, NULL, N'Absent', NULL, 0, NULL, N'NotYetCheckOut', N'Absent', N'Nghỉ không phép')
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (29, 49, 10, CAST(N'2026-07-22T08:57:00.0000000' AS DateTime2), 1, CAST(90.09 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T20:10:00.0000000' AS DateTime2), 1, CAST(94.82 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (30, 50, 13, CAST(N'2026-07-22T07:19:00.0000000' AS DateTime2), 1, CAST(94.00 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T17:47:00.0000000' AS DateTime2), 1, CAST(93.49 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (31, 51, 11, CAST(N'2026-07-22T06:50:00.0000000' AS DateTime2), 1, CAST(91.09 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T20:15:00.0000000' AS DateTime2), 1, CAST(91.89 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (32, 52, 14, CAST(N'2026-07-22T08:43:00.0000000' AS DateTime2), 1, CAST(96.35 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-22T20:40:00.0000000' AS DateTime2), 1, CAST(91.17 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (33, 53, 11, CAST(N'2026-07-23T07:27:00.0000000' AS DateTime2), 1, CAST(95.22 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T19:46:00.0000000' AS DateTime2), 1, CAST(93.73 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (34, 54, 16, CAST(N'2026-07-23T06:14:00.0000000' AS DateTime2), 1, CAST(92.02 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T21:11:00.0000000' AS DateTime2), 1, CAST(96.02 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (35, 55, 12, CAST(N'2026-07-23T06:05:00.0000000' AS DateTime2), 1, CAST(97.10 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T20:02:00.0000000' AS DateTime2), 1, CAST(96.15 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (36, 56, 14, CAST(N'2026-07-23T07:26:00.0000000' AS DateTime2), 1, CAST(94.25 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-23T17:53:00.0000000' AS DateTime2), 1, CAST(91.18 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (37, 57, 14, CAST(N'2026-07-24T06:18:00.0000000' AS DateTime2), 1, CAST(90.27 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T18:24:00.0000000' AS DateTime2), 1, CAST(92.82 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (38, 58, 12, CAST(N'2026-07-24T08:51:00.0000000' AS DateTime2), 1, CAST(97.21 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T15:58:00.0000000' AS DateTime2), 1, CAST(98.67 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (39, 59, 16, CAST(N'2026-07-24T07:53:00.0000000' AS DateTime2), 1, CAST(97.48 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T15:32:00.0000000' AS DateTime2), 1, CAST(95.26 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (40, 60, 10, CAST(N'2026-07-24T07:54:00.0000000' AS DateTime2), 1, CAST(98.58 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-07-24T19:26:00.0000000' AS DateTime2), 1, CAST(94.07 AS Decimal(18, 2)), N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (1002, 1055, 6, CAST(N'2026-08-09T16:29:13.4386012' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (1004, 102, 6, CAST(N'2026-08-10T03:25:41.5015183' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (1007, 1100, 6, CAST(N'2026-08-12T11:31:21.7034393' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2003, 87, 6, CAST(N'2026-08-14T11:15:56.6675685' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2004, 2094, 6, CAST(N'2026-08-15T15:59:54.6028237' AS DateTime2), 1, NULL, N'OnTime', CAST(N'2026-08-15T16:27:02.5743242' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2005, 2124, 5, CAST(N'2026-08-15T16:27:02.5933017' AS DateTime2), 1, NULL, N'OnTime', CAST(N'2026-08-15T20:36:35.2308506' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2006, 2096, 4, CAST(N'2026-08-15T18:42:59.7425944' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2007, 2136, 6, CAST(N'2026-08-15T20:36:35.2904366' AS DateTime2), 1, NULL, N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2008, 2152, 6, CAST(N'2026-08-16T05:40:08.5770085' AS DateTime2), 1, NULL, N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2009, 2157, 7, CAST(N'2026-08-16T07:27:07.6127423' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2011, 2158, 7, CAST(N'2026-08-16T07:27:07.0000000' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2012, 2160, 6, CAST(N'2026-08-16T21:23:50.6475252' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-08-16T22:10:20.4182476' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2013, 2159, 5, CAST(N'2026-08-16T11:51:04.5632930' AS DateTime2), 1, NULL, N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2014, 2171, 5, CAST(N'2026-08-16T22:10:20.4311133' AS DateTime2), 1, NULL, N'OnTime', CAST(N'2026-08-16T22:12:02.0768385' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2015, 1112, 6, CAST(N'2026-08-26T03:07:46.6524047' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2019, 1143, 6, CAST(N'2026-08-26T19:50:32.2542513' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2038, 2174, 7, CAST(N'2026-08-27T14:30:52.2484173' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2040, 2172, 6, CAST(N'2026-08-27T08:48:10.5613424' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2043, 1011, 6, CAST(N'2026-08-27T14:11:02.1583160' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2044, 1029, 6, CAST(N'2026-08-27T23:20:19.8873971' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2048, 1049, 6, CAST(N'2026-08-28T16:58:06.0098759' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2050, 93, 6, CAST(N'2026-08-28T19:35:54.1606262' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2052, 2110, 7, CAST(N'2026-08-29T21:21:19.4287068' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2053, 2142, 6, CAST(N'2026-08-29T23:20:05.7979769' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2055, 2181, 7, CAST(N'2026-09-03T03:46:18.7923511' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2056, 2179, 6, CAST(N'2026-09-03T05:02:56.0834400' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2057, 1033, 6, CAST(N'2026-09-03T13:45:46.5034294' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (2058, 2204, 6, CAST(N'2026-09-04T06:01:25.5900810' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3054, 2178, 6, CAST(N'2026-09-06T06:09:36.3757347' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3055, 2169, 7, CAST(N'2026-09-06T06:12:34.6495527' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3056, 2170, 1004, CAST(N'2026-09-06T06:29:02.7870130' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3057, 2114, 4, CAST(N'2026-09-06T06:29:34.0703336' AS DateTime2), 1, CAST(95.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3058, 2176, 6, CAST(N'2026-09-09T11:54:07.3216158' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3059, 1153, 7, CAST(N'2026-09-09T13:55:11.6215942' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3060, 3178, 6, CAST(N'2026-09-12T00:10:13.5726368' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3061, 2118, 6, CAST(N'2026-09-12T09:52:28.0335102' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3062, 2119, 7, CAST(N'2026-09-12T10:08:27.5357420' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (3063, 2148, 6, CAST(N'2026-09-12T11:15:33.2524469' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (4060, 4178, 6, CAST(N'2026-09-13T12:46:51.5508532' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-09-13T15:13:17.1414145' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (5060, 5178, 6, CAST(N'2026-09-14T15:55:59.9800327' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (5061, 5179, 6, CAST(N'2026-09-15T14:11:55.3229044' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-09-15T14:14:15.1433428' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (5062, 5180, 15, CAST(N'2026-09-15T14:26:33.9274504' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', CAST(N'2026-09-15T14:36:42.1081602' AS DateTime2), 0, NULL, N'CheckedOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (6061, 6179, 6, CAST(N'2026-09-20T13:58:47.5561099' AS DateTime2), 1, NULL, N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (6062, 6182, 14, CAST(N'2026-09-20T14:15:19.0366761' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (6063, 6181, 16, CAST(N'2026-09-20T14:17:28.1787691' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'OnTime', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
INSERT [dbo].[attendance_logs] ([attendance_id], [roster_id], [employee_id], [check_in_time], [is_face_check_in_valid], [check_in_confidence], [check_in_status], [check_out_time], [is_face_check_out_valid], [check_out_confidence], [check_out_status], [overall_status], [notes]) VALUES (6064, 6183, 10, CAST(N'2026-09-20T14:18:47.9517119' AS DateTime2), 1, CAST(100.00 AS Decimal(18, 2)), N'Late', NULL, 0, NULL, N'NotYetCheckOut', N'Present', NULL)
GO
SET IDENTITY_INSERT [dbo].[attendance_logs] OFF
GO

-- -------------------------------------------------------------
-- Data for table: branch_inventories (17 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[branch_inventories] ON
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (1, N'CB004', 1, CAST(679.03 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (2, N'CB004', 2, CAST(361.03 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (3, N'CB004', 3, CAST(191.44 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (4, N'CB004', 4, CAST(159.11 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (5, N'CB004', 5, CAST(119.72 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (6, N'CB004', 6, CAST(61.91 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (7, N'CB004', 7, CAST(63.49 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (8, N'CB004', 8, CAST(66.90 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (9, N'CB005', 1, CAST(499.91 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (10, N'CB005', 2, CAST(199.88 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (11, N'CB005', 3, CAST(149.98 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (12, N'CB005', 4, CAST(49.94 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (13, N'CB005', 5, CAST(50.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (14, N'CB005', 6, CAST(49.97 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (15, N'CB005', 7, CAST(49.99 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (16, N'CB005', 8, CAST(50.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[branch_inventories] ([inventory_id], [branch_id], [material_id], [stock_quantity], [low_stock_threshold]) VALUES (17, N'CB004', 9, CAST(15.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[branch_inventories] OFF
GO

-- -------------------------------------------------------------
-- Data for table: branch_managers (2 rows)
-- -------------------------------------------------------------
INSERT [dbo].[branch_managers] ([branch_id], [manager_id], [appointed_date]) VALUES (N'CB004', 2, CAST(N'2025-09-30T16:17:53.2811023' AS DateTime2))
GO
INSERT [dbo].[branch_managers] ([branch_id], [manager_id], [appointed_date]) VALUES (N'CB005', 9, CAST(N'2025-01-30T16:17:53.2825493' AS DateTime2))
GO

-- -------------------------------------------------------------
-- Data for table: branch_menus (3 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[branch_menus] ON
GO
INSERT [dbo].[branch_menus] ([menu_id], [branch_id], [menu_name], [is_active], [updated_at]) VALUES (1, N'CB004', N'Thực đơn CB004', 1, CAST(N'2026-08-09T08:09:57.1818129' AS DateTime2))
GO
INSERT [dbo].[branch_menus] ([menu_id], [branch_id], [menu_name], [is_active], [updated_at]) VALUES (2, N'CB005', N'Thực đơn CB005', 1, NULL)
GO
INSERT [dbo].[branch_menus] ([menu_id], [branch_id], [menu_name], [is_active], [updated_at]) VALUES (1002, N'CB004', N'dme', 0, CAST(N'2026-08-09T08:08:42.0351243' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[branch_menus] OFF
GO

-- -------------------------------------------------------------
-- Data for table: branch_settings (2 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[branch_settings] ON
GO
INSERT [dbo].[branch_settings] ([setting_id], [branch_id], [branch_display_name], [contact_phone], [address], [opening_hours], [auto_print_receipt], [enable_sound_notification], [bank_code], [account_number], [account_name], [sepay_api_key], [webhook_secret_token], [transfer_prefix], [auto_confirm_order], [is_sepay_active], [updated_at]) VALUES (1, N'CB004', N'Chi nhánh Tân Bình', N'0988888888', N'78 Cộng Hòa, Tân Bình, TPHCM', N'05:30 - 4:30', 1, 1, N'TPBank', N'00000117863', N'CSMS CAFE', N'5TILOKDAPRNB2MDSP0VZSJTRGO5NFFA8EAHJCMOE1XDYK8QLY9OP03XCIUPY6DXG', N'a4d6b25733d58046144d580fa6eebc4fc8f6646efd95d34123e46705e79df967', N'CSMS', 1, 1, CAST(N'2026-08-14T20:04:27.3223889' AS DateTime2))
GO
INSERT [dbo].[branch_settings] ([setting_id], [branch_id], [branch_display_name], [contact_phone], [address], [opening_hours], [auto_print_receipt], [enable_sound_notification], [bank_code], [account_number], [account_name], [sepay_api_key], [webhook_secret_token], [transfer_prefix], [auto_confirm_order], [is_sepay_active], [updated_at]) VALUES (2, N'CB005', N'Chi nhánh Thủ Đức', N'0988888888', N'12 Võ Văn Ngân, Thủ Đức, TPHCM', N'06:30 - 22:30', 1, 1, N'MBBank', N'0333333333', N'CSMS CAFE', N'', N'', N'CSMS', 1, 1, CAST(N'2026-08-15T19:16:42.8324767' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[branch_settings] OFF
GO

-- -------------------------------------------------------------
-- Data for table: branch_supply_request_items (58 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[branch_supply_request_items] ON
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (10, 4, 1, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (11, 4, 2, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (12, 4, 3, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (13, 4, 4, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (14, 5, 1, CAST(1248.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (15, 5, 2, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (16, 5, 4, CAST(14.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (17, 5, 5, CAST(15.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (18, 6, 1, CAST(1.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (19, 6, 2, CAST(1.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (20, 6, 3, CAST(1.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (21, 7, 1, CAST(9.00 AS Decimal(18, 2)), CAST(9.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (22, 8, 1, CAST(50.00 AS Decimal(18, 2)), CAST(50.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (23, 8, 2, CAST(100.00 AS Decimal(18, 2)), CAST(100.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (24, 8, 3, CAST(25.00 AS Decimal(18, 2)), CAST(25.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (25, 8, 4, CAST(56.00 AS Decimal(18, 2)), CAST(56.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (26, 8, 5, CAST(66.00 AS Decimal(18, 2)), CAST(66.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (27, 8, 6, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (28, 8, 7, CAST(14.00 AS Decimal(18, 2)), CAST(14.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (29, 8, 8, CAST(17.00 AS Decimal(18, 2)), CAST(17.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (30, 9, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (31, 9, 2, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (32, 9, 3, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (33, 10, 1, CAST(31.00 AS Decimal(18, 2)), CAST(31.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (34, 10, 4, CAST(31.00 AS Decimal(18, 2)), CAST(31.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (35, 10, 5, CAST(4.00 AS Decimal(18, 2)), CAST(4.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (36, 11, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (37, 12, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(8.60 AS Decimal(18, 2)), CAST(3.40 AS Decimal(18, 2)), N'Hư hại do vận chuyển', NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (38, 12, 2, CAST(24.00 AS Decimal(18, 2)), CAST(24.00 AS Decimal(18, 2)), CAST(24.00 AS Decimal(18, 2)), CAST(19.60 AS Decimal(18, 2)), CAST(4.40 AS Decimal(18, 2)), N'Hư hại do vận chuyển', NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (39, 12, 3, CAST(4.00 AS Decimal(18, 2)), CAST(4.00 AS Decimal(18, 2)), CAST(4.00 AS Decimal(18, 2)), CAST(4.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (40, 13, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (41, 13, 3, CAST(14.00 AS Decimal(18, 2)), CAST(14.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (42, 13, 4, CAST(15.20 AS Decimal(18, 2)), CAST(15.20 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1022, 1008, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1023, 1009, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(7.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), N'Hư hại do vận chuyển', NULL, N'/csms-storage/supply_defects/6cecfc2244654c1d9ec21053584692f9.jpg')
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1024, 1009, 2, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(7.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), N'Hư hại do vận chuyển', NULL, N'/csms-storage/supply_defects/afbd48d09e054faca01e53898fceb5ab.png')
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1025, 1009, 4, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), CAST(7.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), N'Hư hại do vận chuyển', NULL, N'/csms-storage/supply_defects/95adca74799b496a81c96e4199180f1e.png')
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1026, 1010, 1, CAST(13.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1027, 1010, 2, CAST(14.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1028, 1010, 4, CAST(12.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1029, 1011, 1, CAST(12.00 AS Decimal(18, 2)), CAST(11.00 AS Decimal(18, 2)), CAST(11.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), N'Hư hại do vận chuyển', NULL, N'/csms-storage/supply_defects/2652b47f0ff341838c2c92335ffb082c.jpg')
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1030, 1012, 1, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1031, 1012, 2, CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1032, 1013, 3, CAST(24.00 AS Decimal(18, 2)), CAST(24.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1033, 1013, 4, CAST(25.00 AS Decimal(18, 2)), CAST(25.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1034, 1013, 1, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1035, 1013, 2, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1036, 1013, 5, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1037, 1013, 6, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1038, 1013, 7, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1039, 1013, 8, CAST(12.00 AS Decimal(18, 2)), CAST(12.00 AS Decimal(18, 2)), NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1040, 2013, 1, CAST(5.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), CAST(4.00 AS Decimal(18, 2)), CAST(1.00 AS Decimal(18, 2)), N'Không đạt chất lượng', NULL, N'["/uploads/supply_defects/3cfddecb9efd4735bac93850ac183113.jpg","/uploads/supply_defects/4c1cb29fdb224230a2340b8db393cd83.jpg"]')
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1041, 2013, 2, CAST(5.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), CAST(5.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1042, 2014, 1, CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (1043, 2014, 2, CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (2042, 3013, 1, CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (2043, 3013, 2, CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(10.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_request_items] ([request_item_id], [request_id], [material_id], [quantity_requested], [quantity_released], [quantity_received], [quantity_accepted], [quantity_defective], [defect_type], [defect_note], [defect_image_url]) VALUES (2044, 3014, 9, CAST(15.00 AS Decimal(18, 2)), CAST(15.00 AS Decimal(18, 2)), CAST(15.00 AS Decimal(18, 2)), CAST(15.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[branch_supply_request_items] OFF
GO

-- -------------------------------------------------------------
-- Data for table: branch_supply_requests (20 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[branch_supply_requests] ON
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (4, N'YC-004', N'CB004', CAST(N'2026-08-09T11:54:54.3909196' AS DateTime2), N'Đã hủy', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (5, N'YC-005', N'CB004', CAST(N'2026-08-09T14:29:16.5083322' AS DateTime2), N'Đã hủy', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (6, N'YC-006', N'CB004', CAST(N'2026-08-09T14:50:06.2874944' AS DateTime2), N'Đã hủy', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (7, N'YC-007', N'CB004', CAST(N'2026-08-09T15:55:53.0046603' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-08-09T15:56:33.4379445' AS DateTime2), N'easdgfssđ', N'12345423423', CAST(N'2026-08-09T23:36:17.5603068' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (8, N'YC-008', N'CB004', CAST(N'2026-08-09T23:48:44.9012308' AS DateTime2), N'Đã hoàn thành', N'ok', N'Warehouse Manager', CAST(N'2026-08-09T23:49:51.6164416' AS DateTime2), N'asfsfaasdfalskjf', NULL, CAST(N'2026-08-09T23:54:24.9559126' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (9, N'YC-009', N'CB004', CAST(N'2026-08-09T23:54:57.9084859' AS DateTime2), N'Đang chuẩn bị xuất', NULL, N'Warehouse Manager', CAST(N'2026-08-10T00:10:05.0008513' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (10, N'YC-010', N'CB004', CAST(N'2026-08-09T23:55:17.0789903' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-08-10T00:09:24.1111957' AS DateTime2), N'123312341', NULL, CAST(N'2026-08-13T16:09:35.7737959' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (11, N'YC-011', N'CB004', CAST(N'2026-08-10T00:09:40.2386048' AS DateTime2), N'Đã xuất kho', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-08-13T16:34:30.0934872' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (12, N'YC-012', N'CB004', CAST(N'2026-08-12T00:54:23.2096304' AS DateTime2), N'Đã nhận - Có hàng lỗi', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-08-15T20:09:43.4845001' AS DateTime2), NULL, NULL, CAST(N'2026-09-12T13:01:57.5491414' AS DateTime2), NULL, NULL, NULL, NULL, NULL, N'Trần Văn Sơn', CAST(N'2026-09-12T13:01:57.5491404' AS DateTime2), N'Có hàng lỗi')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (13, N'YC-013', N'CB004', CAST(N'2026-08-12T21:59:07.9571023' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-08-12T22:00:07.7333976' AS DateTime2), N'nguoi giao hang', N'12345678', CAST(N'2026-08-12T22:06:19.9497942' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (1008, N'YC-014', N'CB004', CAST(N'2026-08-13T16:09:07.4170113' AS DateTime2), N'Đang chuẩn bị xuất', NULL, N'Warehouse Manager', CAST(N'2026-08-15T20:09:50.8426848' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (1009, N'YC-1009', N'CB004', CAST(N'2026-08-15T20:07:49.7889859' AS DateTime2), N'Đã nhận - Có hàng lỗi', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-09-12T14:02:05.9488380' AS DateTime2), N'123', N'12345432345', CAST(N'2026-09-12T14:03:39.0953648' AS DateTime2), N'12345', NULL, NULL, NULL, NULL, N'Trần Văn Sơn', CAST(N'2026-09-12T14:03:39.0953636' AS DateTime2), N'Có hàng lỗi')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (1010, N'YC-1010', N'CB004', CAST(N'2026-08-16T08:50:55.6499831' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-08-16T08:53:01.6481022' AS DateTime2), NULL, NULL, CAST(N'2026-09-12T12:58:50.1715790' AS DateTime2), NULL, NULL, NULL, NULL, NULL, N'Trần Văn Sơn', CAST(N'2026-09-12T12:58:50.1715375' AS DateTime2), N'Đạt 100%')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (1011, N'YC-1011', N'CB004', CAST(N'2026-09-09T13:58:23.7572800' AS DateTime2), N'Đã nhận - Có hàng lỗi', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-09-12T13:07:26.3138643' AS DateTime2), N'TX-01', N'018264702184', CAST(N'2026-09-12T13:12:01.2913324' AS DateTime2), N'29C-12974', NULL, NULL, NULL, NULL, N'Trần Văn Sơn', CAST(N'2026-09-12T13:12:01.2913311' AS DateTime2), N'Có hàng lỗi')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (1012, N'YC-1012', N'CB004', CAST(N'2026-09-12T12:10:02.4406799' AS DateTime2), N'Chờ duyệt', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (1013, N'YC-1013', N'CB004', CAST(N'2026-09-12T12:57:42.1209134' AS DateTime2), N'Chờ duyệt', NULL, NULL, NULL, NULL, NULL, NULL, NULL, CAST(N'2026-09-13T00:00:00.0000000' AS DateTime2), N'Trần Văn Sơn', N'0900000004', N'qwerty', NULL, NULL, NULL)
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (2013, N'YC-1014', N'CB004', CAST(N'2026-09-13T14:44:28.6030523' AS DateTime2), N'Đã nhận - Có hàng lỗi', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-09-13T14:48:23.6695973' AS DateTime2), N'Quân', N'0900000123', CAST(N'2026-09-13T14:55:53.1898516' AS DateTime2), NULL, CAST(N'2026-09-14T00:00:00.0000000' AS DateTime2), N'Trần Văn Sơn', N'0900000004', N'', N'Trần Văn Sơn', CAST(N'2026-09-13T14:55:53.1898498' AS DateTime2), N'Có hàng lỗi')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (2014, N'YC-1015', N'CB004', CAST(N'2026-09-13T14:58:07.2776103' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-09-13T14:58:31.7903819' AS DateTime2), N'Quân', N'09888888888', CAST(N'2026-09-13T14:59:01.7065983' AS DateTime2), NULL, CAST(N'2026-09-14T00:00:00.0000000' AS DateTime2), N'Trần Văn Sơn', N'0900000004', N'', N'Trần Văn Sơn', CAST(N'2026-09-13T14:59:01.7065970' AS DateTime2), N'Đạt 100%')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (3013, N'YC-1016', N'CB004', CAST(N'2026-09-15T14:01:20.6754266' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-09-15T14:02:58.8727331' AS DateTime2), N'0', N'0904592141', CAST(N'2026-09-15T14:05:59.6009124' AS DateTime2), NULL, CAST(N'2026-09-16T00:00:00.0000000' AS DateTime2), N'Trần Văn Sơn', N'0900000004', N'', N'Trần Văn Sơn', CAST(N'2026-09-15T14:05:59.6008634' AS DateTime2), N'Đạt 100%')
GO
INSERT [dbo].[branch_supply_requests] ([request_id], [request_code], [branch_id], [request_date], [status], [warehouse_note], [approved_by], [approved_date], [deliverer_name], [deliverer_phone], [received_date], [delivery_provider], [expected_delivery_date], [receiver_name], [receiver_phone], [request_note], [inspected_by], [inspected_at], [inspection_status]) VALUES (3014, N'YC-1017', N'CB004', CAST(N'2026-09-19T08:40:44.0935336' AS DateTime2), N'Đã hoàn thành', N'Đang giao hàng', N'Warehouse Manager', CAST(N'2026-09-19T08:57:34.9980136' AS DateTime2), N'Nguyen Giao Hang', N'0192809750', CAST(N'2026-09-19T08:59:10.7241846' AS DateTime2), N'MH-370', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), N'Trần Văn Sơn', N'0900000004', N'cần loại muối bị mốc', N'Trần Văn Sơn', CAST(N'2026-09-19T08:59:10.7240433' AS DateTime2), N'Đạt 100%')
GO
SET IDENTITY_INSERT [dbo].[branch_supply_requests] OFF
GO

-- -------------------------------------------------------------
-- Data for table: branches (2 rows)
-- -------------------------------------------------------------
INSERT [dbo].[branches] ([branch_id], [branch_name], [address], [phone_number], [email], [opening_time], [closing_time], [status]) VALUES (N'CB004', N'Chi nhánh Tân Bình', N'78 Cộng Hòa, Tân Bình, TPHCM', N'0988888888', NULL, CAST(N'06:30:00' AS Time), CAST(N'22:30:00' AS Time), N'Active')
GO
INSERT [dbo].[branches] ([branch_id], [branch_name], [address], [phone_number], [email], [opening_time], [closing_time], [status]) VALUES (N'CB005', N'Chi nhánh Thủ Đức', N'12 Võ Văn Ngân, Thủ Đức, TPHCM', NULL, NULL, CAST(N'07:00:00' AS Time), CAST(N'21:30:00' AS Time), N'Active')
GO

-- -------------------------------------------------------------
-- Data for table: cash_handovers (41 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[cash_handovers] ON
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1, N'CB004', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 2, 2, 3, CAST(500000.00 AS Decimal(18, 2)), CAST(635618.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(923745.00 AS Decimal(18, 2)), CAST(908059.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5535773' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2, N'CB004', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 1, 3, 2, CAST(500000.00 AS Decimal(18, 2)), CAST(1280325.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1353550.00 AS Decimal(18, 2)), CAST(1358756.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538615' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (3, N'CB004', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 3, 3, 2, CAST(500000.00 AS Decimal(18, 2)), CAST(1065770.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1210513.00 AS Decimal(18, 2)), CAST(1205974.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538628' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (4, N'CB004', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 1, 2, 3, CAST(500000.00 AS Decimal(18, 2)), CAST(1188347.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1292232.00 AS Decimal(18, 2)), CAST(1282141.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538637' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (5, N'CB004', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 3, 3, 2, CAST(500000.00 AS Decimal(18, 2)), CAST(2547398.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(2198265.00 AS Decimal(18, 2)), CAST(2181272.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538644' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (6, N'CB005', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 3, 16, 12, CAST(500000.00 AS Decimal(18, 2)), CAST(2691613.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(2294409.00 AS Decimal(18, 2)), CAST(2296451.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538769' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (7, N'CB005', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 1, 12, 15, CAST(500000.00 AS Decimal(18, 2)), CAST(1437694.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1458463.00 AS Decimal(18, 2)), CAST(1460315.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538778' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (8, N'CB005', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 3, 15, 12, CAST(500000.00 AS Decimal(18, 2)), CAST(1531490.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1520994.00 AS Decimal(18, 2)), CAST(1504061.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538785' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (9, N'CB005', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 3, 16, 12, CAST(500000.00 AS Decimal(18, 2)), CAST(2667541.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(2278361.00 AS Decimal(18, 2)), CAST(2290168.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538793' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (10, N'CB005', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 3, 16, 12, CAST(500000.00 AS Decimal(18, 2)), CAST(1773328.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1682218.00 AS Decimal(18, 2)), CAST(1667739.00 AS Decimal(18, 2)), NULL, 1, N'Closed', CAST(N'2026-07-30T16:17:53.5538802' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (11, N'CB004', CAST(N'2026-07-31T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(25000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(25000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Closed', CAST(N'2026-07-31T21:47:11.7105764' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (12, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(1234567890.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1234567890.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Closed', CAST(N'2026-08-03T16:17:41.1195656' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1002, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(15000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(15000000.00 AS Decimal(18, 2)), CAST(20000000.00 AS Decimal(18, 2)), N'232123432234432234', 1, N'Closed', CAST(N'2026-08-06T16:21:00.8170141' AS DateTime2), CAST(N'2026-08-06T16:33:50.1985774' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1003, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(1234321233.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1234321233.00 AS Decimal(18, 2)), CAST(1234412000.00 AS Decimal(18, 2)), N'12312332343234', 1, N'Closed', CAST(N'2026-08-06T16:35:06.1742675' AS DateTime2), CAST(N'2026-08-06T16:35:29.0109486' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1004, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(1234412000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1234412000.00 AS Decimal(18, 2)), CAST(1412123000.00 AS Decimal(18, 2)), N'+177.711.000', 1, N'Closed', CAST(N'2026-08-06T17:48:30.0363163' AS DateTime2), CAST(N'2026-08-06T17:49:20.1111436' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1005, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(200000.00 AS Decimal(18, 2)), N'1234323432', 1, N'Closed', CAST(N'2026-08-07T13:04:12.2835578' AS DateTime2), CAST(N'2026-08-07T13:04:50.4818112' AS DateTime2), N'ehka lkflkasq 1', CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1006, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(300000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(300000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Closed', CAST(N'2026-08-09T16:28:15.5374076' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1007, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 1, 6, 5, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), N'asdfgasdfgsdfgsdfg', 1, N'Closed', CAST(N'2026-08-12T10:57:26.1061818' AS DateTime2), CAST(N'2026-08-12T11:49:50.7063421' AS DateTime2), N'123456787432', CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1008, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 2, 5, 5, CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Closed', CAST(N'2026-08-12T11:51:51.7335306' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (1009, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 1, 6, 5, CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(100000.00 AS Decimal(18, 2)), CAST(80000.00 AS Decimal(18, 2)), N'danh bac', 1, N'Closed', CAST(N'2026-08-12T05:55:49.9861090' AS DateTime2), CAST(N'2026-08-12T06:11:18.4954720' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2007, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Closed', CAST(N'2026-08-13T15:45:56.2604206' AS DateTime2), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Normal', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2008, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 2, 6, 5, CAST(1000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'[BÀN GIAO ĐỘT XUẤT GIỮA CA] Lý do: Gia đình có việc khẩn cấp. ', 1, N'Closed', CAST(N'2026-08-15T15:59:54.3192483' AS DateTime2), CAST(N'2026-08-15T16:01:30.3810601' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'Emergency', N'Gia đình có việc khẩn cấp', CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2009, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 2, 5, 6, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'[BÀN GIAO ĐỘT XUẤT GIỮA CA] Lý do: Nhân viên ốm/sốt đột xuất. ', 1, N'Closed', CAST(N'2026-08-15T16:01:30.4678424' AS DateTime2), CAST(N'2026-08-15T16:03:43.8156029' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'Emergency', N'Nhân viên ốm/sốt đột xuất', CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2010, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 2, 6, 5, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(25000.00 AS Decimal(18, 2)), CAST(-25000.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)), N'lech tien', 1, N'Closed', CAST(N'2026-08-15T16:03:43.8415154' AS DateTime2), CAST(N'2026-08-15T16:27:02.5639036' AS DateTime2), N'Vu Mih Giag', CAST(25000.00 AS Decimal(18, 2)), N'MidShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2011, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 5, 6, CAST(1000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)), CAST(1120000.00 AS Decimal(18, 2)), N'khách đưa tiền mặt', 1, N'Closed', CAST(N'2026-08-15T16:27:02.5868533' AS DateTime2), CAST(N'2026-08-15T20:36:35.1090169' AS DateTime2), NULL, CAST(0.00 AS Decimal(18, 2)), N'MidShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2012, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(1120000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1084000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Closed', CAST(N'2026-08-15T20:36:35.2584850' AS DateTime2), NULL, NULL, CAST(36000.00 AS Decimal(18, 2)), N'MidShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2027, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 1, 6, 6, CAST(1120000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1120000.00 AS Decimal(18, 2)), CAST(1120000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-08-16T08:31:29.7294418' AS DateTime2), CAST(N'2026-08-16T16:31:29.7294418' AS DateTime2), N'Võ Quốc Quân', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2030, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 1, 6, 6, CAST(10000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(10000000.00 AS Decimal(18, 2)), CAST(10000000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-08-27T07:47:56.4348162' AS DateTime2), CAST(N'2026-08-27T15:47:56.4348162' AS DateTime2), N'Võ Quốc Quân', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2031, N'CB004', CAST(N'2026-08-28T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(5000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-08-28T16:57:57.0448421' AS DateTime2), CAST(N'2026-08-29T00:57:57.0448421' AS DateTime2), N'Võ Quốc Quân', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2032, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(5000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-08-29T21:17:49.2847969' AS DateTime2), CAST(N'2026-08-30T05:17:49.2847969' AS DateTime2), N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2033, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 1, 6, 6, CAST(5125000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5125000.00 AS Decimal(18, 2)), CAST(5125000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-09-03T03:39:10.2205280' AS DateTime2), CAST(N'2026-09-03T11:39:10.2205280' AS DateTime2), N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (2034, N'CB004', CAST(N'2026-09-04T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(5125000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5125000.00 AS Decimal(18, 2)), CAST(5125000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-09-04T06:02:26.1635840' AS DateTime2), CAST(N'2026-09-04T14:02:26.1635840' AS DateTime2), N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (3033, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 2, 6, 6, CAST(5125000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-09-06T06:09:55.5654054' AS DateTime2), CAST(N'2026-09-06T14:09:55.5654054' AS DateTime2), N'Vũ Minh Giang', CAST(99000.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (3034, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(5026000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-09-09T11:54:14.7126572' AS DateTime2), CAST(N'2026-09-09T19:54:14.7126572' AS DateTime2), N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (3035, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 1, 6, 6, CAST(5026000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-09-12T00:10:20.4508113' AS DateTime2), CAST(N'2026-09-12T08:10:20.4508113' AS DateTime2), N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (3036, N'CB004', CAST(N'2026-09-13T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(5026000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5026000.00 AS Decimal(18, 2)), CAST(5418600.00 AS Decimal(18, 2)), N'[ĐÓNG CA CUỐI NGÀY - Tổng két chốt: 5,418,600đ, Để lại két ngày mai: 5,000,000đ, Nộp két tổng: 418,600đ, Người nhận tiền: Trần Văn Sơn] quản lí đánh bạc', 1, N'Closed', CAST(N'2026-09-13T12:47:16.8416138' AS DateTime2), CAST(N'2026-09-13T15:13:17.0819792' AS DateTime2), N'Trần Văn Sơn', CAST(0.00 AS Decimal(18, 2)), N'LastShift', NULL, CAST(5000000.00 AS Decimal(18, 2)), CAST(418600.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (4036, N'CB004', CAST(N'2026-09-14T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(5000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), N' [TỰ ĐỘNG ĐÓNG CA QUÁ HẠN KHI MỞ NGÀY MỚI]', 0, N'Closed', CAST(N'2026-09-14T15:56:29.5884030' AS DateTime2), CAST(N'2026-09-14T23:56:29.5884030' AS DateTime2), N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (4037, N'CB004', CAST(N'2026-09-15T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(5000000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), CAST(5000000.00 AS Decimal(18, 2)), N'[ĐÓNG CA CUỐI NGÀY - Tổng két chốt: 5,000,000đ, Để lại két ngày mai: 4,440,000đ, Nộp két tổng: 560,000đ, Người nhận tiền: Trần Văn SơnTr] lấy đi', 1, N'Closed', CAST(N'2026-09-15T14:12:14.1298658' AS DateTime2), CAST(N'2026-09-15T14:14:15.1159952' AS DateTime2), N'Trần Văn SơnTr', CAST(0.00 AS Decimal(18, 2)), N'LastShift', NULL, CAST(4440000.00 AS Decimal(18, 2)), CAST(560000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (4038, N'CB005', CAST(N'2026-09-15T00:00:00.0000000' AS DateTime2), 3, 15, 15, CAST(1667739.00 AS Decimal(18, 2)), CAST(22220.00 AS Decimal(18, 2)), CAST(36780.00 AS Decimal(18, 2)), CAST(1689959.00 AS Decimal(18, 2)), CAST(1689959.00 AS Decimal(18, 2)), N'[ĐÓNG CA CUỐI NGÀY - Tổng két chốt: 1,689,959đ, Để lại két ngày mai: 0đ, Nộp két tổng: 1,689,959đ, Người nhận tiền: bmanager41] không có', 1, N'Closed', CAST(N'2026-09-15T14:26:45.1339398' AS DateTime2), CAST(N'2026-09-15T14:36:42.0768599' AS DateTime2), N'bmanager41', CAST(0.00 AS Decimal(18, 2)), N'LastShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(1689959.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (5037, N'CB004', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 6, 6, CAST(4440000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(4440000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Active', CAST(N'2026-09-20T13:58:47.4342983' AS DateTime2), NULL, N'Vũ Minh Giang', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[cash_handovers] ([handover_id], [branch_id], [handover_date], [shift_id], [outgoing_cashier_id], [incoming_cashier_id], [initial_cash], [machine_cash_revenue], [bank_transfer_revenue], [theoretical_cash], [actual_cash], [notes], [is_password_confirmed], [status], [opened_at], [closed_at], [deliverer_name], [cash_refund_amount], [handover_type], [emergency_reason], [retained_cash], [deposited_cash]) VALUES (5038, N'CB005', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 16, 16, CAST(1689959.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(1689959.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, 0, N'Active', CAST(N'2026-09-20T14:17:41.9500307' AS DateTime2), NULL, N'Hồ Thị Vy', CAST(0.00 AS Decimal(18, 2)), N'FirstShift', NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[cash_handovers] OFF
GO

-- -------------------------------------------------------------
-- Data for table: employees (20 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[employees] ON
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (1, N'rmanager', N'10000.+f6j25DzC7Ry4lszxncx9Q==.hVPwsQ4nMG8I3Fkj6NICEoq14zH+heTYe01w6NNtL+0=', N'Regional Manager', CAST(N'1990-01-01T00:00:00.0000000' AS DateTime2), N'Headquarters', N'0900000000', N'rmanager@gmail.com', N'RManager', N'Full-time', N'000000000000', NULL, NULL, N'Active', NULL, 0, NULL, NULL)
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (2, N'bmanager41', N'10000.Ek1Y1zicQQdCnLuC8nm8PA==.omUwQL4EjOq8FaQ/5TzYz98VAdsLPxWpJoRZUWtcABM=', N'Trần Văn Sơn', CAST(N'1992-10-23T00:00:00.0000000' AS DateTime2), N'Địa chỉ quản lý CB004', N'0900000004', N'manager4@gmail.com', N'BranchManager', N'Full-time', N'000000000004', NULL, NULL, N'Active', NULL, 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (3, N'busser44', N'10000.lG5znK/ZpzGWXacwclpq+Q==.I896EjoPtcO5hznTtxxyUbe2naFfIbu4BY3uYMDiAEM=', N'Phạm Thanh Em', CAST(N'1998-03-01T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB004', N'0900000005', N'emp005@gmail.com', N'Busser', N'Full-time', N'000000000005', NULL, NULL, N'Active', NULL, 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (4, N'busser41', N'10000.Fq9pM7bi3pZ/of1bjnfbUg==.hOMyDnJVjIo/WjgSP4H3Arf3fgfNJJEL11I2unyB5c0=', N'Lê Ngọc Uyên', CAST(N'1997-02-15T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB004', N'0900000006', N'emp006@gmail.com', N'Busser', N'Full-time', N'000000000006', NULL, NULL, N'Active', N'[-0.005284156650304794,0.04393387213349342,0.07247841358184814,-0.10551802068948746,-0.11575357615947723,-0.03438137471675873,-0.08746946603059769,-0.0796537846326828,0.1504264920949936,-0.09433247894048691,0.21766169369220734,-0.03260447829961777,-0.21125644445419312,-0.0897916853427887,-0.05718682333827019,0.1205858439207077,-0.0881255567073822,-0.1031126156449318,0.03359484672546387,0.004506864584982395,0.05172182619571686,0.05620246380567551,0.05368170887231827,0.02430013194680214,-0.07036639004945755,-0.28828704357147217,-0.12390442192554474,-0.12506960332393646,-0.07343538850545883,-0.06063808128237724,-0.05975296348333359,0.043724387884140015,-0.13696856796741486,-0.06978815793991089,0.05182698741555214,0.08625772595405579,-0.07051653414964676,-0.07615065574645996,0.17297284305095673,0.04943925142288208,-0.19402945041656494,-0.07279805094003677,0.019822197034955025,0.22554269433021545,0.10967060923576355,0.04204066842794418,0.007802041247487068,-0.1341089904308319,0.12175584584474564,-0.1415332704782486,0.08313384652137756,0.15366022288799286,0.11252400279045105,0.023188181221485138,0.06067405641078949,-0.08363168686628342,0.10676118731498718,0.19176873564720154,-0.19694820046424866,-0.028914012014865875,0.058244481682777405,-0.04590687155723572,-0.04480304941534996,-0.060630787163972855,0.27159202098846436,0.1365521103143692,-0.08213268220424652,-0.15886275470256805,0.15893691778182983,-0.07098513841629028,-0.15320274233818054,0.0769590437412262,-0.17126917839050293,-0.15632937848567963,-0.2265906184911728,0.005587805062532425,0.3463919162750244,0.07170489430427551,-0.1837238371372223,0.039487265050411224,-0.06321029365062714,-0.004826624412089586,0.11991842091083527,0.09502150118350983,-0.040763966739177704,0.03207037225365639,-0.07800193130970001,-0.059494707733392715,0.16894462704658508,-0.10294287651777267,-0.01700788363814354,0.13096441328525543,-0.028516508638858795,0.07698748260736465,0.058719005435705185,-0.037752144038677216,-0.055152907967567444,0.09769386798143387,-0.1648700088262558,-0.035246461629867554,0.059540435671806335,-0.05105161294341087,-0.033629160374403,0.11747602373361588,-0.21455763280391693,0.0601748563349247,0.025112541392445564,-0.0825435221195221,0.02657930925488472,-0.08989286422729492,-0.08603957295417786,-0.06623880565166473,0.1459856629371643,-0.25019168853759766,0.19838029146194458,0.09907793998718262,0.05677112936973572,0.13495637476444244,0.09639634937047958,0.06862714886665344,-0.01706220582127571,-0.11707103252410889,-0.2254640907049179,-0.02225126326084137,0.14445529878139496,-0.0004958244971930981,0.04668469354510307,-0.03249470144510269]', 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (5, N'busser42', N'10000.GRLcrVHiVuf41hyW8EhRzA==.rkbhJZTeHcUXXmbL0n3BQLn1TSXtSmJQq1zjhP/MfEY=', N'Võ Quốc Quân', CAST(N'2000-05-07T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB004', N'0900000007', N'emp007@gmail.com', N'Busser', N'Part-time', N'000000000007', N'/csms-storage/employees/f1963498e4c64036a614aae0943933c8.png', N'/csms-storage/employees/5e74a12e656345bea866a58dddf5fbe0.png', N'Active', NULL, 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (6, N'cashier41', N'10000.RA+BXlJ+MaG9RLHrQymGxw==.QzdqJ0nt16lw0qQOU+6M/OhAcchImVAtfJm2OhdukXU=', N'Vũ Minh Giang', CAST(N'2001-01-14T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB004', N'0900000008', N'emp008@gmail.com', N'Cashier', N'Full-time', N'000000000008', N'/csms-storage/employees/contract_6_2eb6bb12-61e3-4144-8488-6c20ae47dd90.png', N'/csms-storage/employees/cccd_6_112de7ef-7fab-43a1-a95f-93f3dfd69a93.png', N'Active', N'[-0.04349520802497864,0.17969776690006256,0.04617571085691452,0.01670275814831257,-0.0490611307322979,-0.07618747651576996,-0.011748526245355606,-0.16213354468345642,0.1594468057155609,-0.02945893257856369,0.22933802008628845,-0.012959275394678116,-0.17381396889686584,-0.13305237889289856,0.013739216141402721,0.14086437225341797,-0.1955340951681137,-0.10217896848917007,-0.05394173413515091,-0.022586103528738022,0.01091746985912323,-0.05040876939892769,0.0794454887509346,0.05389529466629028,0.0144643634557724,-0.33050528168678284,-0.10107458382844925,-0.11498797684907913,0.14583483338356018,-0.048976071178913116,-0.10960359126329422,0.01823919638991356,-0.19282759726047516,-0.08412382751703262,-0.00476648285984993,0.039564333856105804,-0.07411853224039078,-0.01639888621866703,0.22433635592460632,-0.04377782344818115,-0.16530998051166534,0.045357123017311096,0.03495943546295166,0.22329917550086975,0.18032041192054749,0.07064405828714371,0.06698644161224365,-0.1260785609483719,0.0357491560280323,-0.06419000029563904,0.050532884895801544,0.21775896847248077,0.10459502041339874,0.11529137194156647,-0.02553960494697094,-0.17592555284500122,0.0015155645087361336,0.02365710400044918,-0.09790322929620743,0.06397794187068939,0.13141542673110962,-0.06894068419933319,-0.04286273568868637,-0.0143924281001091,0.2912759482860565,0.0420035757124424,-0.10464553534984589,-0.16047385334968567,0.09078504145145416,-0.16396500170230865,-0.07803982496261597,0.0371132418513298,-0.14696811139583588,-0.15211406350135803,-0.26351550221443176,0.06355816125869751,0.34644564986228943,0.12670044600963593,-0.17460483312606812,0.05454464256763458,-0.15306657552719116,-0.051378171890974045,0.0831993892788887,0.16097724437713623,-0.08700518310070038,0.06583444774150848,-0.11778334528207779,0.00038465578109025955,0.14968039095401764,-0.06890805065631866,0.01129026897251606,0.22553838789463043,0.009188334457576275,0.10512489080429077,-0.04836447536945343,-0.010326803661882877,-0.04532874375581741,0.039723943918943405,-0.12530554831027985,-0.04540468007326126,0.05356555059552193,-0.06778649985790253,0.014151142910122871,0.08042988926172256,-0.18664157390594482,0.08216308802366257,0.044828351587057114,0.06146339327096939,0.027749698609113693,0.04143541306257248,-0.09382806718349457,-0.11651642620563507,0.13702277839183807,-0.22849959135055542,0.2811061441898346,0.2168431580066681,0.08314721286296844,0.09428191184997559,0.13007105886936188,0.1404532492160797,0.011781483888626099,-0.06040658429265022,-0.13021278381347656,0.020921317860484123,0.07317115366458893,0.004911895375698805,0.060083527117967606,0.055365074425935745]', 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (7, N'bartender41', N'10000.UJUrWorKoqudCI0Y5HGFwQ==.oOCOuxlvGwXZo8+HhW3edhuPfzZV8Nfr9Ge3R0uC4Bo=', N'Bùi Minh Giang', CAST(N'1997-08-08T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB004', N'0900000009', N'emp009@gmail.com', N'Bartender', N'Full-time', N'000000000009', NULL, NULL, N'Active', NULL, 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (8, N'busser43', N'10000.IbhFn7NSEE1ELlwpoOOfKg==.5ACbvGpBcuMfc5ONXESO0Ju7R+XbzCRNrQP66vX0CEM=', N'Lê Ngọc Oanh', CAST(N'1999-06-14T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB004', N'0900000010', N'emp010@gmail.com', N'Busser', N'Full-time', N'000000000010', NULL, NULL, N'Active', NULL, 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (9, N'bmanager51', N'10000.VKO8U0YpmcwWP72ve767Ng==.HbvjC9clbW2ToDFChQHykmiR6RbIX9wa7ECaSw4PBC4=', N'Võ Kim Bình', CAST(N'1995-03-03T00:00:00.0000000' AS DateTime2), N'Địa chỉ quản lý CB005', N'0900000011', N'manager11@gmail.com', N'BranchManager', N'Full-time', N'000000000011', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (10, N'bartender52', N'10000.B5WHV5FNw4UsmZoLDGmrlw==.r+sSOmILautk2AtTsFo59rlbtfLqARI1CE2WpV7ixNQ=', N'Nguyễn Ngọc An', CAST(N'2001-09-21T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000012', N'emp012@gmail.com', N'Bartender', N'Full-time', N'000000000012', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (11, N'busser52', N'10000.6wGYeZNfQFnL/+dt8ZwZKg==.N7jLUQuRooeeGZ4knaxi/TcIk5c2fU1FRUnJ8UrriXI=', N'Bùi Minh Hùng', CAST(N'2000-08-08T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000013', N'emp013@gmail.com', N'Busser', N'Full-time', N'000000000013', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (12, N'cashier52', N'10000.g7ZinC8LVWausZJHBe0syQ==.dpZdErXfUj6DKT4ovnWmxmcH2jBLPmiZXiaxLoZrtbo=', N'Hoàng Hữu Uyên', CAST(N'1999-07-17T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000014', N'emp014@gmail.com', N'Cashier', N'Full-time', N'000000000014', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (13, N'busser51', N'10000.6LXHEFMyZXRXu+d05qc86w==.f3887maET4mgl1t6xdpN33K3OIGOLr0EOo7SakvZBu4=', N'Nguyễn Minh Quân', CAST(N'1995-12-15T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000015', N'emp015@gmail.com', N'Busser', N'Full-time', N'000000000015', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (14, N'bartender51', N'10000.b/BXIbnfrWtKfhN4b/mOaQ==.+LXWsHi36paHJGlogKWUNNZMGr0Nr9mJ9O9LvHWCMiE=', N'Hoàng Thị Giang', CAST(N'1998-08-12T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000016', N'emp016@gmail.com', N'Bartender', N'Part-time', N'000000000016', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (15, N'cashier51', N'10000.aWKmqecPEszaZI1bJezRlA==.CMjbBl9oDPTmMRfDz8/iShNgVElluLD+/9X82o1Pn4c=', N'Hồ Thị Vy', CAST(N'1996-01-23T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000017', N'emp017@gmail.com', N'Cashier', N'Full-time', N'000000000017', NULL, NULL, N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (16, N'cashier53', N'10000.IvlHaEz+BHTfJ2FBPOk6NA==.iJXNJmVgz8Ydxim1khRIAE3yXyDMsjHlC8r7gUcRLeU=', N'Đặng Xuân Oanh', CAST(N'2002-03-23T00:00:00.0000000' AS DateTime2), N'Địa chỉ nhân viên CB005', N'0900000018', N'emp018@gmail.com', N'Cashier', N'Part-time', N'000000000018', N'/csms-storage/employees/contract_16_b5df016e-ac9d-47c5-8035-c2bc41915186.png', N'/csms-storage/employees/cccd_16_2d82cf9c-8572-453b-b38e-c78b0f46d4c6.png', N'Active', NULL, 0, NULL, N'CB005')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (1002, N'wmanager', N'10000.8YAz3O7iru7ds99TQsoMdA==.sfhQeD06OKtU6BDO31pI8SHaf0l7IpyxDnoN+vls4V8=', N'Warehouse Manager', CAST(N'1992-01-01T00:00:00.0000000' AS DateTime2), N'Central Warehouse', N'0911111111', N'wmanager@gmail.com', N'WarehouseManager', N'Full-time', N'111111111111', NULL, NULL, N'Active', NULL, 0, NULL, NULL)
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (1003, N'nguyenvana', N'10000.cPb3Oau1bn9D4ycVQkgs2Q==.HAfLaUMNuU0x1okotElHjyrk0vzLSIB9K7BBuaPPrkw=', N'Nguyen Van A', CAST(N'2006-08-15T00:00:00.0000000' AS DateTime2), N'Chưa cập nhật', N'09128098121', N'nguyenvana@gmail.com', N'Cashier', N'Full-time', N'123456789012', NULL, NULL, N'Active', N'[-0.07073325663805008,0.09474796056747437,0.047777846455574036,-0.04581785574555397,-0.017979655414819717,-0.0688335970044136,-0.029789544641971588,-0.06499286741018295,0.1374591886997223,-0.11473631113767624,0.2211744785308838,-0.06973033398389816,-0.23919172585010529,-0.09628143906593323,0.021576451137661934,0.13136596977710724,-0.24661703407764435,-0.09470535069704056,-0.04933303967118263,0.0027090227231383324,0.16513533890247345,0.030214762315154076,-0.07249170541763306,0.02833070047199726,-0.07775349915027618,-0.37053537368774414,-0.11422798782587051,-0.035621076822280884,0.07504488527774811,-0.0500425323843956,-0.04683946818113327,-0.025701472535729408,-0.1744833141565323,-0.09676693379878998,0.028456544503569603,0.07957728207111359,-0.011112483218312263,-0.055606547743082047,0.13506083190441132,0.0115522975102067,-0.2216227501630783,0.050690073519945145,0.02132769674062729,0.24138447642326355,0.18305188417434692,0.07086602598428726,0.01328308042138815,-0.1542520672082901,0.12483600527048111,-0.13919077813625336,0.01741575449705124,0.17813314497470856,0.13782836496829987,0.08732813596725464,-0.00014368537813425064,-0.17171049118041992,-0.021507281810045242,0.13695448637008667,-0.19331641495227814,0.06043141707777977,0.061270371079444885,-0.12615850567817688,0.03380019590258598,-0.061928484588861465,0.14145903289318085,0.006999127566814423,-0.14968903362751007,-0.18426965177059174,0.12249112874269485,-0.172722727060318,-0.06870736181735992,0.028145328164100647,-0.12763024866580963,-0.18232178688049316,-0.3421788513660431,0.018792087212204933,0.401735782623291,0.062251508235931396,-0.20390336215496063,0.03038609027862549,-0.052627116441726685,-0.05121506005525589,0.15447472035884857,0.20077279210090637,-0.007372419349849224,0.018702007830142975,-0.07888085395097733,0.021267350763082504,0.20177021622657776,-0.040727946907281876,-0.02484470047056675,0.1821717470884323,-0.03841505944728851,0.1040438860654831,0.02757800742983818,0.027135204523801804,-0.11291714012622833,0.024936139583587646,-0.10504355281591415,-0.012728055939078331,-0.01707986369729042,-0.10869329422712326,-0.02106117643415928,0.11949018388986588,-0.12163516134023666,0.156852126121521,0.04958512261509895,0.023552805185317993,-0.004064793232828379,-0.006232734303921461,-0.10432607680559158,-0.06133368983864784,0.14297425746917725,-0.21276000142097473,0.22002193331718445,0.2280559539794922,0.06941799819469452,0.146836519241333,0.09440883249044418,0.08809465914964676,-0.00847761332988739,-0.0259241946041584,-0.2263021469116211,-0.006005964707583189,0.0359957180917263,0.0019598393701016903,0.10429570823907852,-0.043423641473054886]', 1, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (1004, N'nguyenngocquan', N'10000.81yk1Czu+KUxcnUH0U0wWQ==.NWKeHtowVed6KdMg3D/g+MiEj7DoWTWGBtnyMrxP31c=', N'Nguyễn Ngọc Quân', CAST(N'2006-08-15T00:00:00.0000000' AS DateTime2), N'Chưa cập nhật', N'0985587762', N'quannnhe176871@fpt.edu.vn', N'Bartender', N'Part-time', N'012345678912', NULL, NULL, N'Active', N'[-0.09658150374889374,0.11248119175434113,-0.016098158434033394,-0.10835891962051392,-0.054988179355859756,-0.05114735662937164,-0.09068096429109573,-0.08665595203638077,0.15557605028152466,-0.06261016428470612,0.2388269454240799,-0.03934476524591446,-0.18358828127384186,-0.17234444618225098,-0.0865306556224823,0.18820461630821228,-0.22620801627635956,-0.1671598255634308,0.02451269142329693,-0.020088793709874153,0.06380882114171982,-0.03316688537597656,0.004582416731864214,0.10179296135902405,-0.13007546961307526,-0.3319995701313019,-0.045316774398088455,-0.13317464292049408,-0.07208161801099777,-0.06343312561511993,-0.044990137219429016,0.14099617302417755,-0.2083042860031128,-0.04258335009217262,0.003570729400962591,0.0835094302892685,0.017665032297372818,0.03529104217886925,0.16303041577339172,0.023301905021071434,-0.2178107500076294,0.01341775432229042,0.0010305361356586218,0.2533462941646576,0.1496298909187317,0.01871212013065815,0.023366615176200867,-0.11462503671646118,0.11334460973739624,-0.17223381996154785,0.020151596516370773,0.18550120294094086,0.09961527585983276,0.02809164859354496,-0.08753891289234161,-0.12232525646686554,0.0322292223572731,0.08230416476726532,-0.18520092964172363,-0.012857606634497643,-0.001210773829370737,-0.1406533122062683,-0.04402733966708183,-0.05458379536867142,0.2188982516527176,0.11400248855352402,-0.10782874375581741,-0.17050084471702576,0.1358305662870407,-0.11690644174814224,-0.0830366462469101,0.09482228755950928,-0.13769592344760895,-0.1963367909193039,-0.3478573262691498,0.03549223020672798,0.365800678730011,0.08167016506195068,-0.16862940788269043,0.06612401455640793,-0.08762399107217789,-0.03455688804388046,0.07072993367910385,0.14012667536735535,-0.08433084189891815,0.06030968204140663,-0.08663580566644669,0.01337589230388403,0.20604321360588074,-0.047659605741500854,-0.06196640804409981,0.17601411044597626,-0.01536786649376154,0.14322350919246674,0.023156315088272095,0.030148770660161972,-0.009153696708381176,0.05036122351884842,-0.11270067095756531,0.02266480214893818,-0.01750568486750126,-0.04498163238167763,-0.050435617566108704,0.09453205019235611,-0.14892974495887756,0.06415927410125732,0.024442680180072784,-0.010821064934134483,-0.05045280605554581,0.03746951371431351,-0.055723778903484344,-0.13314494490623474,0.10765925794839859,-0.20759698748588562,0.20234201848506927,0.19907546043395996,0.12270559370517731,0.15668298304080963,0.12235839664936066,0.03700998052954674,-0.038025274872779846,-0.09972478449344635,-0.1703668236732483,0.017002606764435768,0.16716690361499786,-0.016225816681981087,0.0890224277973175,-0.01254852395504713]', 0, NULL, N'CB004')
GO
INSERT [dbo].[employees] ([employee_id], [username], [password], [full_name], [date_of_birth], [address], [phone_number], [email], [role], [employment_type], [citizen_id], [contract_file_path], [cccd_file_path], [status], [face_data], [failed_login_attempts], [lockout_until], [branch_id]) VALUES (1005, N'nguyenvana1', N'10000.0dFDru6V53roNZofhPAobg==.+shcS2WLioD+XNNXqvRl8GPL2eGsuc7OtfxmEHBceSU=', N'nguyễn văn a', CAST(N'2006-08-15T00:00:00.0000000' AS DateTime2), N'Chưa cập nhật', N'0397321948', N'datmthe176706@fpt.edu.vn', N'Cashier', N'Full-time', N'038203021855', NULL, NULL, N'Active', NULL, 1, NULL, N'CB004')
GO
SET IDENTITY_INSERT [dbo].[employees] OFF
GO

-- -------------------------------------------------------------
-- Data for table: fixed_shifts (3 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[fixed_shifts] ON
GO
INSERT [dbo].[fixed_shifts] ([shift_id], [shift_name], [start_time], [end_time]) VALUES (1, N'Ca 1', CAST(N'07:00:00' AS Time), CAST(N'12:00:00' AS Time))
GO
INSERT [dbo].[fixed_shifts] ([shift_id], [shift_name], [start_time], [end_time]) VALUES (2, N'Ca 2', CAST(N'13:00:00' AS Time), CAST(N'18:00:00' AS Time))
GO
INSERT [dbo].[fixed_shifts] ([shift_id], [shift_name], [start_time], [end_time]) VALUES (3, N'Ca 3', CAST(N'18:00:00' AS Time), CAST(N'23:59:59' AS Time))
GO
SET IDENTITY_INSERT [dbo].[fixed_shifts] OFF
GO

-- -------------------------------------------------------------
-- Data for table: leave_applications (30 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[leave_applications] ON
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1, 7, CAST(N'2026-08-18T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2, 6, CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-05T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (3, 16, CAST(N'2026-08-02T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-04T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (4, 6, CAST(N'2026-08-08T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (5, 13, CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), N'Pending', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (6, 10, CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (7, 10, CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (8, 14, CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), N'Rejected', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (9, 5, CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-05T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (10, 16, CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (11, 7, CAST(N'2026-08-05T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), N'Rejected', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (12, 14, CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), N'Rejected', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (13, 10, CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), N'Rejected', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (14, 8, CAST(N'2026-08-08T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), N'Rejected', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (15, 3, CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), N'Việc gia đình', NULL, CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1002, 6, CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), N'12377654345677654', N'ca1', CAST(N'2026-08-09T14:22:20.3034449' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1003, 6, CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), N'1234543454345523y544', N'ca2', CAST(N'2026-08-09T14:22:32.3019728' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1004, 4, CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), N'12389', N'12367', CAST(N'2026-08-09T14:45:25.6291069' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1005, 6, CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), N'1234234', N'1234234', CAST(N'2026-08-10T03:28:55.2721849' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1006, 6, CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), N'nghir', N'c2', CAST(N'2026-08-12T00:30:23.8440178' AS DateTime2), N'Rejected', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1007, 7, CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), N'met', N'ca ngay', CAST(N'2026-08-12T11:39:57.2411005' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2005, 6, CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), N'ot ac', N'ca to', CAST(N'2026-08-14T11:33:46.2056044' AS DateTime2), N'Rejected', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2006, 4, CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), N'h', N'ca1', CAST(N'2026-08-15T19:00:11.6906436' AS DateTime2), N'Rejected', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2007, 13, CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), N'ăn', N'ca1', CAST(N'2026-08-15T19:03:56.6379006' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2008, 13, CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), N'mệt', N'ca1', CAST(N'2026-08-15T19:10:30.8409829' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2009, 13, CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), N'ytci', N'ca1', CAST(N'2026-08-15T19:18:06.1351555' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2010, 6, CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), N'ốm', N'ca 1', CAST(N'2026-08-15T23:49:24.5372645' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2011, 6, CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), N'12', N'12', CAST(N'2026-08-16T06:29:11.2508926' AS DateTime2), N'Pending', NULL, NULL)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2012, 6, CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), N'nghỉ', N'ca1', CAST(N'2026-09-15T14:15:39.2387373' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[leave_applications] ([application_id], [employee_id], [start_date], [end_date], [reason], [leave_shifts], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2013, 6, CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), N'nghỉ', N'ca1', CAST(N'2026-09-15T14:16:06.4493587' AS DateTime2), N'Approved', N'CB004', 2)
GO
SET IDENTITY_INSERT [dbo].[leave_applications] OFF
GO

-- -------------------------------------------------------------
-- Data for table: master_products (24 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[master_products] ON
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (1, N'Cà phê đen', N'/csms-storage/products/f7194337600a4ae28eba3617e208c9bf.webp', N'Cà phê đen thơm ngon', 1, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (2, N'Cà phê sữa', N'/csms-storage/products/f4ea3aff60a542c2a623aac2a65b5e10.jpg', N'Cà phê sữa thơm ngon', 1, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (3, N'Bạc xỉu', N'/csms-storage/products/c586cea115534fe0b3980cee83383c26.jpg', N'Bạc xỉu thơm ngon', 1, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (4, N'Espresso', N'/csms-storage/products/b379f8a18cc94be2999beb98b7b59a52.jpg', N'Espresso thơm ngon', 1, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (5, N'Trà đào cam sả', N'/csms-storage/products/ce0f71dce83147238b0a5f91a10078f0.webp', N'Trà đào cam sả thơm ngon', 2, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (6, N'Trà vải', N'/csms-storage/products/53117fa401f94ab3a8f8ec97715bdc99.jpg', N'Trà vải thơm ngon', 2, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (7, N'Trà sen vàng', N'/csms-storage/products/4670ec888ff2452ab38c9cbff5f52e15.jpg', N'Trà sen vàng thơm ngon', 2, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (8, N'Trà oolong', N'/csms-storage/products/7ef3b973373443ae84ada1878eef5989.webp', N'Trà oolong thơm ngon', 2, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (9, N'Nước ép cam', N'/csms-storage/products/910e10b2fab34ecabccc0bfd3403c43e.webp', N'Nước ép cam thơm ngon', 3, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (10, N'Nước ép dưa hấu', N'/csms-storage/products/06642ceec5b64794ada23eb3b6f35d0a.jpg', N'Nước ép dưa hấu thơm ngon', 3, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (11, N'Nước ép táo', N'/csms-storage/products/25db2ffb641c45c7a4ea3a780aa431d8.webp', N'Nước ép táo thơm ngon', 3, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (12, N'Sinh tố bơ', N'/csms-storage/products/bc3832fd37ba4f66bc9fc0e956af2dfb.jpg', N'Sinh tố bơ thơm ngon', 3, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (13, N'Frappuccino cà phê', N'/csms-storage/products/a9fb64032ef047fa8371ddbbd098859a.webp', N'Frappuccino cà phê thơm ngon', 4, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (14, N'Matcha đá xay', N'/csms-storage/products/69ef631e9d854543a3eddb2332fce6a0.jpg', N'Matcha đá xay thơm ngon', 4, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (15, N'Chocolate đá xay', N'/csms-storage/products/327d75fb5b834ff18751078d93a1ce19.webp', N'Chocolate đá xay thơm ngon', 4, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (16, N'Caramel đá xay', N'/csms-storage/products/304f396063694a448b3764633426600e.jpg', N'Caramel đá xay thơm ngon', 4, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (17, N'Bánh croissant', N'/csms-storage/products/c11918b54c854b7d8bcee5641a0b59c7.webp', N'Bánh croissant thơm ngon', 5, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (18, N'Bánh tiramisu', N'/csms-storage/products/5ec8c5954c5d4a1fa1999e539d7f091a.jpg', N'Bánh tiramisu thơm ngon', 5, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (19, N'Bánh su kem', N'/csms-storage/products/9573e83fc0c54e60910e4a94bf788ca3.jpg', N'Bánh su kem thơm ngon', 5, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (20, N'Bánh cookie', N'/csms-storage/products/d761d681706f410985c08a9312344b68.jpg', N'Bánh cookie thơm ngon', 5, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (21, N'Trân châu đen', N'/csms-storage/products/b4fc7c48c9a04c0d87f3f53b7827797f.png', N'Trân châu đen thơm ngon', 6, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (22, N'Thạch dừa', N'/csms-storage/products/81ba62e94f7c474ca390f43455523aed.jpg', N'Thạch dừa thơm ngon', 6, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (23, N'Pudding trứng', N'/csms-storage/products/5b9518fa0b4d4e7ab8bbf2b36264ab0d.jpg', N'Pudding trứng thơm ngon', 6, N'Active')
GO
INSERT [dbo].[master_products] ([product_id], [product_name], [image_url], [description], [category_id], [status]) VALUES (24, N'Kem cheese', N'/csms-storage/products/cc2950985d88436cbbc03e587de8f260.jpg', N'Kem cheese thơm ngon', 6, N'Active')
GO
SET IDENTITY_INSERT [dbo].[master_products] OFF
GO

-- -------------------------------------------------------------
-- Data for table: material_categories (3 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[material_categories] ON
GO
INSERT [dbo].[material_categories] ([category_id], [category_name], [description]) VALUES (1, N'Cà phê', N'Các loại nguyên liệu cà phê thô hoặc hạt')
GO
INSERT [dbo].[material_categories] ([category_id], [category_name], [description]) VALUES (2, N'Sữa', N'Sữa đặc, sữa tươi và chế phẩm từ sữa')
GO
INSERT [dbo].[material_categories] ([category_id], [category_name], [description]) VALUES (3, N'Gia vị', N'Đường, muối, bột ngọt, siro hương vị')
GO
SET IDENTITY_INSERT [dbo].[material_categories] OFF
GO

-- -------------------------------------------------------------
-- Data for table: materials (9 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[materials] ON
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (1, N'NL001', N'Cà phê hạt', N'Thô', N'Cà phê', N'Trung Nguyên', CAST(150000.00 AS Decimal(18, 2)), N'Buôn Ma Thuột', N'kg', N'Dạng đặc', CAST(938.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (2, N'NL002', N'Sữa đặc', N'Thành phẩm', N'Sữa', N'Vinamilk', CAST(35000.00 AS Decimal(18, 2)), NULL, N'lít', N'Dạng lỏng', CAST(39.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (3, N'NL003', N'Đường cát trắng', N'Thô', N'Gia vị', N'Đường Biên Hòa', CAST(20000.00 AS Decimal(18, 2)), NULL, N'kg', N'Dạng đặc', CAST(107.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (4, N'NL004', N'Sữa tươi ít đường', N'Thành phẩm', N'Sữa', N'Dalat Milk', CAST(42000.00 AS Decimal(18, 2)), NULL, N'lít', N'Dạng lỏng', CAST(184.80 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (5, N'NL005', N'Hạt sen tươi', N'Thô', N'Gia vị', N'Sen Việt', CAST(85000.00 AS Decimal(18, 2)), N'Đồng Tháp', N'kg', N'Dạng đặc', CAST(70.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (6, N'NL006', N'Đào ngâm', N'Thành phẩm', N'Gia vị', N'Kronos', CAST(65000.00 AS Decimal(18, 2)), NULL, N'kg', N'Dạng đặc', CAST(208.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (7, N'NL007', N'Trà Oolong túi lọc', N'Thô', N'Cà phê', N'Phúc Long', CAST(120000.00 AS Decimal(18, 2)), NULL, N'kg', N'Dạng đặc', CAST(86.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (8, N'NL008', N'Siro Bạc hà', N'Thành phẩm', N'Gia vị', N'Monin', CAST(180000.00 AS Decimal(18, 2)), NULL, N'lít', N'Dạng lỏng', CAST(100.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[materials] ([material_id], [material_code], [material_name], [material_kind], [category], [supplier], [unit_price], [origin], [storage_unit], [physical_state], [stock_quantity]) VALUES (9, N'NL009', N'Muối', N'thành phẩm', N'Gia vị', N'Quân', CAST(20000.00 AS Decimal(18, 2)), N'không', N'kg', N'Dạng đặc', CAST(53.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[materials] OFF
GO

-- -------------------------------------------------------------
-- Data for table: menu_details (135 rows)
-- -------------------------------------------------------------
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 1, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 2, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 3, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 4, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 5, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 6, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 7, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 8, 1, 7, CAST(N'2026-09-12T11:31:59.5218717' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 9, 1, 7, CAST(N'2026-09-12T11:31:59.5218717' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 10, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 11, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 12, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 13, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 14, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 15, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 16, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 17, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 18, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 19, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 20, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 21, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 22, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 23, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 24, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 25, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 26, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 27, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 28, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 29, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 30, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 31, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 32, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 33, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 34, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 35, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 36, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 37, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 38, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 39, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 40, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 41, 0, 6, CAST(N'2026-09-12T11:38:58.5408302' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 42, 0, 6, CAST(N'2026-09-12T11:38:58.5408302' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 43, 0, 6, CAST(N'2026-09-12T11:38:58.5408302' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 44, 0, 7, CAST(N'2026-09-12T11:21:27.2141962' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 45, 0, 7, CAST(N'2026-09-12T11:21:27.2141962' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 46, 0, 7, CAST(N'2026-09-12T11:21:27.2141962' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 47, 0, 7, CAST(N'2026-09-12T11:53:12.0502050' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 48, 0, 7, CAST(N'2026-09-12T11:53:12.0502050' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 49, 0, 7, CAST(N'2026-09-12T11:21:27.8115156' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 50, 0, 7, CAST(N'2026-09-12T11:21:27.8115156' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 51, 0, 7, CAST(N'2026-09-12T11:21:27.8115156' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 52, 0, 2, CAST(N'2026-09-13T14:30:41.2079048' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 53, 0, 2, CAST(N'2026-09-13T14:30:41.2079048' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 54, 0, 2, CAST(N'2026-09-13T14:30:41.2079048' AS DateTime2))
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 55, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 56, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 57, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 58, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 59, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 60, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 61, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 62, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1, 63, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 1, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 2, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 3, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 4, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 5, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 6, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 8, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 9, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 11, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 12, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 13, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 14, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 15, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 17, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 18, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 19, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 20, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 21, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 22, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 23, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 24, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 25, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 26, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 27, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 28, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 29, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 31, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 34, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 36, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 37, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 38, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 39, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 40, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 41, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 42, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 43, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 46, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 47, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 48, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 49, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 50, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 53, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 54, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 55, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 56, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 57, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 58, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 60, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 61, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (2, 63, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 1, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 2, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 3, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 4, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 5, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 6, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 7, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 10, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 11, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 12, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 16, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 17, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 18, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 19, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 20, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 24, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 25, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 26, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 60, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 61, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 62, 1, NULL, NULL)
GO
INSERT [dbo].[menu_details] ([menu_id], [variant_id], [is_available], [updated_by], [updated_at]) VALUES (1002, 63, 1, NULL, NULL)
GO

-- -------------------------------------------------------------
-- Data for table: notifications (140 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[notifications] ON
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-004.', CAST(N'2026-08-09T11:54:54.5250309' AS DateTime2), 1, NULL, N'WarehouseManager', NULL, NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-005.', CAST(N'2026-08-09T14:29:16.5235002' AS DateTime2), 1, NULL, N'WarehouseManager', NULL, NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-006.', CAST(N'2026-08-09T14:50:06.2920318' AS DateTime2), 1, NULL, N'WarehouseManager', NULL, NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (4, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-007.', CAST(N'2026-08-09T15:55:53.1357162' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (5, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-007 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-09T15:56:33.4391765' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (6, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-007 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-09T15:56:40.0214357' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (7, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã nhận hàng thành công và cập nhật tồn kho cho đơn YC-007.', CAST(N'2026-08-09T23:36:17.5700218' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (8, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-008.', CAST(N'2026-08-09T23:48:45.0438997' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (9, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-008 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-09T23:49:51.6290976' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (10, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-008 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-09T23:51:15.1668247' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (11, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã nhận hàng thành công và cập nhật tồn kho cho đơn YC-008.', CAST(N'2026-08-09T23:54:24.9563824' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (12, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-009.', CAST(N'2026-08-09T23:54:57.9185486' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (13, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-010.', CAST(N'2026-08-09T23:55:17.0872459' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (14, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-010 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-10T00:09:24.1174397' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (15, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-011.', CAST(N'2026-08-10T00:09:40.2473338' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (16, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-009 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-10T00:10:05.0088901' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (17, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-010 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-10T00:10:16.8840309' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (18, N'Đơn xin nghỉ phép mới', N'Nhân viên Vũ Minh Giang đã nộp đơn xin nghỉ phép ngày 13/08/2026.', CAST(N'2026-08-12T00:30:23.9598772' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (19, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-012.', CAST(N'2026-08-12T00:54:23.2298352' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (20, N'Đơn xin nghỉ phép mới', N'Nhân viên Bùi Minh Giang đã nộp đơn xin nghỉ phép ngày 13/08/2026.', CAST(N'2026-08-12T11:39:57.4418736' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (21, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-013.', CAST(N'2026-08-12T21:59:07.9688297' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (22, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-013 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-12T22:00:07.7388957' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (23, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-013 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-12T22:00:16.9665888' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (24, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã nhận hàng thành công và cập nhật tồn kho cho đơn YC-013.', CAST(N'2026-08-12T22:06:19.9499422' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1007, N'Đơn xin nghỉ phép đã được duyệt', N'Đơn xin nghỉ phép ngày 13/08/2026 của bạn đã được phê duyệt.', CAST(N'2026-08-13T16:05:36.9158623' AS DateTime2), 1, 7, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1008, N'Đơn xin nghỉ phép bị từ chối', N'Đơn xin nghỉ phép ngày 13/08/2026 của bạn đã bị từ chối.', CAST(N'2026-08-13T16:05:41.2502076' AS DateTime2), 1, 6, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1009, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-014.', CAST(N'2026-08-13T16:09:07.4333685' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1010, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã nhận hàng thành công và cập nhật tồn kho cho đơn YC-010.', CAST(N'2026-08-13T16:09:35.7744856' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1011, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-011 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-13T16:34:30.0991077' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1012, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-011 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-13T16:34:37.5767035' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1013, N'Đơn xin nghỉ phép mới', N'Nhân viên Vũ Minh Giang đã nộp đơn xin nghỉ phép ngày 15/08/2026.', CAST(N'2026-08-14T11:33:46.4286430' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1014, N'Đơn xin nghỉ phép bị từ chối', N'Đơn xin nghỉ phép ngày 15/08/2026 của bạn đã bị từ chối.', CAST(N'2026-08-14T12:35:45.4941825' AS DateTime2), 1, 6, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1015, N'Đơn xin nghỉ phép mới', N'Nhân viên Lê Ngọc Uyên đã nộp đơn xin nghỉ phép ngày 16/08/2026.', CAST(N'2026-08-15T19:00:11.8754543' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1016, N'Đơn xin nghỉ phép bị từ chối', N'Đơn xin nghỉ phép ngày 16/08/2026 của bạn đã bị từ chối.', CAST(N'2026-08-15T19:02:05.2403639' AS DateTime2), 0, 4, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1017, N'Đơn xin nghỉ phép mới', N'Nhân viên Nguyễn Minh Quân đã nộp đơn xin nghỉ phép ngày 16/08/2026.', CAST(N'2026-08-15T19:03:56.7735432' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1018, N'Đơn xin nghỉ phép đã được duyệt', N'Đơn xin nghỉ phép ngày 16/08/2026 của bạn đã được phê duyệt.', CAST(N'2026-08-15T19:08:52.7303158' AS DateTime2), 1, 13, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1019, N'Đơn xin nghỉ phép mới', N'Nhân viên Nguyễn Minh Quân đã nộp đơn xin nghỉ phép ngày 17/08/2026.', CAST(N'2026-08-15T19:10:30.8526350' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1020, N'Đơn xin nghỉ phép đã được duyệt', N'Đơn xin nghỉ phép ngày 17/08/2026 của bạn đã được phê duyệt.', CAST(N'2026-08-15T19:11:15.9625518' AS DateTime2), 1, 13, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1021, N'Đơn xin nghỉ phép mới', N'Nhân viên Nguyễn Minh Quân đã nộp đơn xin nghỉ phép ngày 17/08/2026.', CAST(N'2026-08-15T19:18:06.1671942' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1022, N'Đơn xin nghỉ phép đã được duyệt', N'Đơn xin nghỉ phép ngày 17/08/2026 của bạn đã được phê duyệt.', CAST(N'2026-08-15T19:18:29.0100323' AS DateTime2), 1, 13, NULL, N'/LeaveRequest', NULL)
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1023, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-1009.', CAST(N'2026-08-15T20:07:49.9070126' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1024, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-012 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-15T20:09:43.4904193' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1025, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-014 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-15T20:09:50.8470966' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1026, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-012 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-15T20:10:14.3161851' AS DateTime2), 1, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1027, N'⚠️ Đơn MH260815-005 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê đen (S x1). Số tiền cần xử lý/hoàn: 36,000 đ. Lý do: hết cà phê', CAST(N'2026-08-15T23:38:49.8168820' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260815-005', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1028, N'⚠️ Đơn MH260815-006 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Espresso (S x1). Số tiền cần xử lý/hoàn: 30,000 đ. Lý do: hết cà phê', CAST(N'2026-08-15T23:42:36.5344599' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260815-006', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1029, N'Đơn xin nghỉ phép mới', N'Nhân viên Vũ Minh Giang đã nộp đơn xin nghỉ phép ngày 16/08/2026.', CAST(N'2026-08-15T23:49:24.5951198' AS DateTime2), 1, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1030, N'Đơn xin nghỉ phép đã được duyệt', N'Đơn xin nghỉ phép ngày 16/08/2026 của bạn đã được phê duyệt.', CAST(N'2026-08-15T23:52:13.6003443' AS DateTime2), 1, 6, NULL, N'/LeaveRequest', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1031, N'Đơn xin nghỉ phép mới', N'Nhân viên Vũ Minh Giang đã nộp đơn xin nghỉ phép ngày 17/08/2026.', CAST(N'2026-08-16T06:29:11.3986916' AS DateTime2), 0, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1032, N'⚠️ Đơn MH260816-002 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê sữa (S x1). Số tiền cần xử lý/hoàn: 29,000 đ. Lý do: ', CAST(N'2026-08-16T07:54:21.9391271' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260816-002', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1033, N'✅ Đơn MH260816-002 đã đổi món thành công', N'Thu ngân đã cập nhật lại danh sách món cho đơn MH260816-002. Đơn đã sẵn sàng để pha chế lại.', CAST(N'2026-08-16T07:55:17.2765198' AS DateTime2), 1, NULL, N'Bartender', N'/Brewing/Index', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1034, N'⚠️ Đơn MH260816-003 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê đen (S x1). Số tiền cần xử lý/hoàn: 36,000 đ. Lý do: thieu cf', CAST(N'2026-08-16T07:57:46.9919393' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260816-003', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1035, N'⚠️ Đơn MH260816-005 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê đen (S x1), Bạc xỉu (S x1). Số tiền cần xử lý/hoàn: 67,000 đ. Lý do: ', CAST(N'2026-08-16T08:53:25.5750211' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260816-005', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1036, N'⚠️ Đơn MH260816-008 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê sữa (S x1). Số tiền cần xử lý/hoàn: 29,000 đ. Lý do: ', CAST(N'2026-08-16T08:39:50.3907838' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260816-008', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1037, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-1010.', CAST(N'2026-08-16T08:50:55.6904810' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1038, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-1010 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-08-16T08:53:01.6560926' AS DateTime2), 0, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1039, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-1010 đã được xuất kho và đang trên đường giao tới chi nhánh.', CAST(N'2026-08-16T08:53:07.1057540' AS DateTime2), 0, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1040, N'⚠️ Đơn MH260816-010 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê sữa (S x1), Espresso (S x1). Số tiền cần xử lý/hoàn: 59,000 đ. Lý do: ', CAST(N'2026-08-27T14:31:25.8843668' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260816-010', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1041, N'⚠️ Đơn MH260906-002 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê sữa (S x1). Số tiền cần xử lý/hoàn: 29,000 đ. Lý do: uyb', CAST(N'2026-09-06T06:41:52.7682185' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260906-002', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1042, N'⚠️ Đơn MH260903-005 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Sinh tố bơ (S x1). Số tiền cần xử lý/hoàn: 25,000 đ. Lý do: ', CAST(N'2026-09-06T06:42:44.3606350' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260903-005', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1043, N'⚠️ Đơn MH260903-002 - Thiếu nguyên liệu pha chế!', N'Bartender báo thiếu món: Cà phê sữa (S x1). Số tiền cần xử lý/hoàn: 29,000 đ. Lý do: ', CAST(N'2026-09-06T06:42:48.4413352' AS DateTime2), 1, 6, N'Cashier', N'/SaleManagement/OrderHistory?search=MH260903-002', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1044, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-1011.', CAST(N'2026-09-09T13:58:23.7862870' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1045, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:16:13.9930085' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1046, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:16:15.6825819' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1047, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:16:16.7929709' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1048, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:16:18.0450181' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1049, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:16:59.3648847' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1050, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:17:00.4420238' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1051, N'⚠️ Món [Bánh su kem] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh su kem] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:17:07.0348457' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1052, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:17:12.9499534' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1053, N'⚠️ Món [Bánh croissant] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh croissant] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:17:14.8500346' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1054, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:21:12.1907253' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1055, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:12.7730699' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1056, N'⚠️ Món [Bánh tiramisu] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh tiramisu] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:14.1081535' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1057, N'✅ Món [Bánh tiramisu] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh tiramisu].', CAST(N'2026-09-12T11:21:14.6822593' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1058, N'✅ Món [Bánh su kem] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh su kem].', CAST(N'2026-09-12T11:21:17.0391732' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1059, N'✅ Món [Bánh croissant] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh croissant].', CAST(N'2026-09-12T11:21:18.1457209' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1060, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:21:18.9155279' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1061, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:19.3824047' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1062, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:21:19.8411200' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1063, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:20.2466934' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1064, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-12T11:21:20.7711925' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1065, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:21.1229789' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1066, N'⚠️ Món [Bánh croissant] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh croissant] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:27.2193600' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1067, N'⚠️ Món [Bánh su kem] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh su kem] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:27.8163669' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1068, N'⚠️ Món [Bánh tiramisu] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh tiramisu] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:21:28.6691244' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1069, N'⚠️ Món [Espresso] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Espresso] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:31:57.1804520' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1070, N'✅ Món [Espresso] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Espresso].', CAST(N'2026-09-12T11:31:59.5341715' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1071, N'⚠️ Món [Caramel đá xay] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Caramel đá xay] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:32:04.4225430' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1072, N'✅ Món [Caramel đá xay] đã phục vụ trở lại', N'Vũ Minh Giang đã mở lại món [Caramel đá xay].', CAST(N'2026-09-12T11:38:57.5678240' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1073, N'⚠️ Món [Caramel đá xay] tạm ngưng phục vụ (Hết NL)', N'Vũ Minh Giang đã đánh dấu tạm hết món [Caramel đá xay] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:38:58.5526346' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1074, N'✅ Món [Bánh tiramisu] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh tiramisu].', CAST(N'2026-09-12T11:53:11.1632955' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1075, N'⚠️ Món [Bánh tiramisu] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh tiramisu] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-12T11:53:12.0569930' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1076, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-1012.', CAST(N'2026-09-12T12:10:02.4808272' AS DateTime2), 1, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1077, N'Yêu cầu xuất kho mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi một yêu cầu xuất kho mới với mã đơn: YC-1013.', CAST(N'2026-09-12T12:57:42.2494216' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1078, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đạt 100% và nhập kho thành công cho đơn YC-1010.', CAST(N'2026-09-12T12:58:50.1717446' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1079, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-012 và phát hiện 7.8 sản phẩm lỗi: Cà phê hạt (3.4 kg - Hư hại do vận chuyển); Sữa đặc (4.4 lít - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:01:57.5491608' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1080, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-012: Cà phê hạt (3.4 kg - Hư hại do vận chuyển); Sữa đặc (4.4 lít - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:01:57.5616440' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1081, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-1011 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-09-12T13:07:26.3199495' AS DateTime2), 0, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1082, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-1011 đã được xuất kho và đang được tài xế TX-01 (29C-12974) (SĐT: 018264702184) giao tới chi nhánh.', CAST(N'2026-09-12T13:08:10.4408782' AS DateTime2), 0, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1083, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1011 và phát hiện 1 sản phẩm lỗi: Cà phê hạt (1 kg - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:11:31.6572343' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1084, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1011: Cà phê hạt (1 kg - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:11:31.6781274' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1085, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1011 và phát hiện 1 sản phẩm lỗi: Cà phê hạt (1 kg - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:11:38.4002395' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1086, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1011: Cà phê hạt (1 kg - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:11:38.4030041' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1087, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1011 và phát hiện 1 sản phẩm lỗi: Cà phê hạt (1 kg - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:12:01.2913496' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1088, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1011: Cà phê hạt (1 kg - Hư hại do vận chuyển).', CAST(N'2026-09-12T13:12:01.2936769' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1089, N'Yêu cầu nhập kho đã duyệt', N'Đơn yêu cầu YC-1009 của chi nhánh bạn đã được duyệt và đang chuẩn bị xuất kho.', CAST(N'2026-09-12T14:02:05.9528073' AS DateTime2), 0, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1090, N'Đơn hàng đang giao', N'Đơn yêu cầu YC-1009 đã được xuất kho và đang được tài xế 123 (12345) (SĐT: 12345432345) giao tới chi nhánh.', CAST(N'2026-09-12T14:02:20.8459659' AS DateTime2), 0, 2, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1091, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1009 và phát hiện 15 sản phẩm lỗi: Cà phê hạt (5 kg - Hư hại do vận chuyển); Sữa đặc (5 lít - Hư hại do vận chuyển); Sữa tươi ít đường (5 lít - Hư hại do vận chuyển).', CAST(N'2026-09-12T14:03:39.0953787' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (1092, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1009: Cà phê hạt (5 kg - Hư hại do vận chuyển); Sữa đặc (5 lít - Hư hại do vận chuyển); Sữa tươi ít đường (5 lít - Hư hại do vận chuyển).', CAST(N'2026-09-12T14:03:39.1067674' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2045, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Vũ Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-13T12:52:12.7178120' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2046, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Vũ Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-13T12:52:13.9721713' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2047, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Bùi Minh Giang đã mở lại món [Bánh cookie].', CAST(N'2026-09-13T14:12:37.4423932' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2048, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Bùi Minh Giang đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-13T14:12:38.2184126' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2049, N'✅ Món [Bánh cookie] đã phục vụ trở lại', N'Trần Văn Sơn đã mở lại món [Bánh cookie].', CAST(N'2026-09-13T14:30:40.3486671' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2050, N'⚠️ Món [Bánh cookie] tạm ngưng phục vụ (Hết NL)', N'Trần Văn Sơn đã đánh dấu tạm hết món [Bánh cookie] do hết nguyên liệu. Món đã được khóa tại màn hình bán hàng.', CAST(N'2026-09-13T14:30:41.2262738' AS DateTime2), 1, NULL, N'Cashier', N'/SaleManagement/CreateOrder', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2051, N'Yêu cầu nhập hàng mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi đơn yêu cầu nhập hàng mới (YC-1014) với 2 nguyên liệu.', CAST(N'2026-09-13T14:44:28.7276141' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2052, N'Đơn yêu cầu đã được duyệt', N'Tổng kho đã duyệt đơn yêu cầu YC-1014 và đang chuẩn bị xuất kho.', CAST(N'2026-09-13T14:48:23.6765934' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2053, N'Hàng đang được vận chuyển', N'Đơn hàng YC-1014 đã được xuất kho bởi tài xế Quân (SĐT: 0900000123). Vui lòng chuẩn bị đồng kiểm khi nhận hàng.', CAST(N'2026-09-13T14:51:15.0340627' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2054, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1014 và phát hiện 1 sản phẩm lỗi: Cà phê hạt (1 kg - Không đạt chất lượng).', CAST(N'2026-09-13T14:54:47.5231925' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2055, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1014: Cà phê hạt (1 kg - Không đạt chất lượng).', CAST(N'2026-09-13T14:54:47.5428113' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2056, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1014 và phát hiện 1 sản phẩm lỗi: Cà phê hạt (1 kg - Không đạt chất lượng).', CAST(N'2026-09-13T14:54:58.6607239' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2057, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1014: Cà phê hạt (1 kg - Không đạt chất lượng).', CAST(N'2026-09-13T14:54:58.6672634' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2058, N'Cảnh báo: Đơn nhận có hàng lỗi', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đơn YC-1014 và phát hiện 1 sản phẩm lỗi: Cà phê hạt (1 kg - Không đạt chất lượng).', CAST(N'2026-09-13T14:55:53.1898648' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2059, N'Báo cáo lỗi nhận hàng chi nhánh', N'Chi nhánh Chi nhánh Tân Bình ghi nhận hàng giao bị lỗi cho đơn YC-1014: Cà phê hạt (1 kg - Không đạt chất lượng).', CAST(N'2026-09-13T14:55:53.2043311' AS DateTime2), 0, NULL, N'RManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2060, N'Yêu cầu nhập hàng mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi đơn yêu cầu nhập hàng mới (YC-1015) với 2 nguyên liệu.', CAST(N'2026-09-13T14:58:07.3055003' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2061, N'Đơn yêu cầu đã được duyệt', N'Tổng kho đã duyệt đơn yêu cầu YC-1015 và đang chuẩn bị xuất kho.', CAST(N'2026-09-13T14:58:31.7966106' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2062, N'Hàng đang được vận chuyển', N'Đơn hàng YC-1015 đã được xuất kho bởi tài xế Quân (SĐT: 09888888888). Vui lòng chuẩn bị đồng kiểm khi nhận hàng.', CAST(N'2026-09-13T14:58:43.4822313' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (2063, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đạt 100% và nhập kho thành công cho đơn YC-1015.', CAST(N'2026-09-13T14:59:01.7066201' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3045, N'Yêu cầu nhập hàng mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi đơn yêu cầu nhập hàng mới (YC-1016) với 2 nguyên liệu.', CAST(N'2026-09-15T14:01:21.7141040' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3046, N'Đơn yêu cầu đã được duyệt', N'Tổng kho đã duyệt đơn yêu cầu YC-1016 và đang chuẩn bị xuất kho.', CAST(N'2026-09-15T14:02:58.8949704' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3047, N'Hàng đang được vận chuyển', N'Đơn hàng YC-1016 đã được xuất kho bởi tài xế 0 (SĐT: 0904592141). Vui lòng chuẩn bị đồng kiểm khi nhận hàng.', CAST(N'2026-09-15T14:05:02.4307024' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3048, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đạt 100% và nhập kho thành công cho đơn YC-1016.', CAST(N'2026-09-15T14:05:59.6009656' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3049, N'Đơn xin nghỉ phép mới', N'Nhân viên Vũ Minh Giang đã nộp đơn xin nghỉ phép ngày 17/09/2026.', CAST(N'2026-09-15T14:15:39.3091092' AS DateTime2), 0, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3050, N'Đơn xin nghỉ phép mới', N'Nhân viên Vũ Minh Giang đã nộp đơn xin nghỉ phép ngày 17/09/2026.', CAST(N'2026-09-15T14:16:06.4626504' AS DateTime2), 0, NULL, N'BranchManager', N'/LeaveRequest/ManagerIndex', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3051, N'Đơn xin nghỉ phép đã được duyệt', N'Đơn xin nghỉ phép ngày 17/09/2026 của bạn đã được phê duyệt.', CAST(N'2026-09-15T14:16:22.7716512' AS DateTime2), 1, 6, NULL, N'/LeaveRequest', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3052, N'Yêu cầu nhập hàng mới', N'Chi nhánh Chi nhánh Tân Bình đã gửi đơn yêu cầu nhập hàng mới (YC-1017) với 1 nguyên liệu.', CAST(N'2026-09-19T08:40:44.2494657' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3053, N'Đơn yêu cầu đã được duyệt', N'Tổng kho đã duyệt đơn yêu cầu YC-1017 và đang chuẩn bị xuất kho.', CAST(N'2026-09-19T08:57:35.0070467' AS DateTime2), 0, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3054, N'Hàng đang được vận chuyển', N'Đơn hàng YC-1017 đã được xuất kho bởi tài xế Nguyen Giao Hang (SĐT: 0192809750). Vui lòng chuẩn bị đồng kiểm khi nhận hàng.', CAST(N'2026-09-19T08:58:02.7409875' AS DateTime2), 1, NULL, N'BranchManager', N'/BranchWarehouse/RequestHistory', N'CB004')
GO
INSERT [dbo].[notifications] ([notification_id], [title], [message], [created_time], [is_read], [recipient_user_id], [recipient_role], [resource_url], [branch_id]) VALUES (3055, N'Nhập kho hoàn tất', N'Chi nhánh Chi nhánh Tân Bình đã đồng kiểm đạt 100% và nhập kho thành công cho đơn YC-1017.', CAST(N'2026-09-19T08:59:10.7245058' AS DateTime2), 0, NULL, N'WarehouseManager', N'/Warehouse/ExportRequests', N'CB004')
GO
SET IDENTITY_INSERT [dbo].[notifications] OFF
GO

-- -------------------------------------------------------------
-- Data for table: order_items (392 rows)
-- -------------------------------------------------------------
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260731-001', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260731-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260731-002', 6, 3, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260731-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260731-003', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260731-003', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-001', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-002', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-003', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-003', 8, 4, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-004', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-004', 8, 4, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-005', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-005', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-006', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-006', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-007', 3, 4, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-007', 8, 11, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-008', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-008', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-009', 3, 103, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-009', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-010', 3, 103, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-010', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-011', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-012', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-012', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260806-013', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-001', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-001', 8, 5, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-002', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-003', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-003', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-004', 1, 3, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-005', 3, 4, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-005', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-005', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-006', 3, 4, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-006', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-006', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-007', 3, 4, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-007', 9, 30, CAST(35000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-007', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-008', 3, 4, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-008', 9, 30, CAST(35000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-008', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-009', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-009', 8, 10, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-010', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-010', 8, 10, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-010', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-010', 30, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 3, 4, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 8, 11, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 13, 4, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 16, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 27, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260807-011', 30, 3, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-001', 1, 4, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-001', 5, 3, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-002', 1, 5, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-003', 3, 5, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-004', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-004', 2, 1, CAST(41000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-004', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-004', 37, 1, CAST(44000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-004', 38, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-004', 39, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 16, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 27, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260809-005', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-001', 1, 3, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-001', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-001', 8, 2, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-002', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-002', 1002, 1, CAST(100000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 2, 1, CAST(41000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 8, 3, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 16, 4, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 42, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 43, 1, CAST(35000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-003', 1002, 1, CAST(100000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-004', 1, 5, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-005', 3, 5, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-005', 16, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-005', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-005', 27, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-005', 30, 2, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-006', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-006', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-006', 16, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-006', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260812-007', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-001', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-002', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-002', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-002', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-003', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-003', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-003', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-004', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-004', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-004', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-005', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-005', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-005', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-006', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-006', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-006', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-007', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-007', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-007', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-008', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-008', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-008', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-009', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-009', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-009', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-010', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-010', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-010', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-011', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-011', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-011', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-012', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-012', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-012', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-013', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-013', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-013', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 13, 4, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 18, 3, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 24, 3, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-014', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 13, 4, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 14, 2, CAST(43000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 18, 3, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 24, 3, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-015', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 1, 2, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 6, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 13, 6, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 18, 3, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 24, 3, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-016', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-017', 18, 4, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-018', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-018', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-019', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-019', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260813-020', 3, 5, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-001', 3, 5, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 3, 6, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 13, 1, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-002', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-003', 24, 3, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-004', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-004', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-004', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-005', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-005', 13, 3, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-005', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-006', 52, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-007', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-008', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-009', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-010', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-011', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260814-012', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-001', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-002', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-003', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-004', 38, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-005', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-005', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-006', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260815-006', 13, 1, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-001', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-002', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-003', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-003', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-004', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-004', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-005', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-005', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-006', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-006', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-006', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-006', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-007', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-007', 9, 2, CAST(35000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-008', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-008', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-009', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-009', 18, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-009', 24, 2, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-010', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260816-010', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-001', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-001', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-002', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-003', 4, 1, CAST(34000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-003', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-004', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-004', 4, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-004', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-004', 9, 1, CAST(35000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-004', 1002, 1, CAST(100000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-005', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-005', 9, 1, CAST(35000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-006', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 16, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 27, 1, CAST(39000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 33, 1, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260827-007', 49, 1, CAST(37000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-001', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-002', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-003', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-004', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-005', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-006', 52, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-007', 52, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 27, 1, CAST(39000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 30, 3, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-008', 52, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-009', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260903-009', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-001', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-001', 13, 1, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-002', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-003', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-004', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260906-004', 8, 2, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 3, 3, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 8, 5, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 23, 1, CAST(41000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 24, 2, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 33, 1, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 35, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-001', 42, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-002', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-002', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-002', 30, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-003', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260909-003', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-001', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-001', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-002', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-002', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-003', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-003', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-004', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-004', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-005', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-005', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-006', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-006', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-007', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-007', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-008', 13, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-008', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-009', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-009', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260912-009', 18, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-001', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-001', 4, 1, CAST(34000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-001', 13, 1, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-001', 14, 1, CAST(43000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-001', 15, 1, CAST(48000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-002', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-002', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-003', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-003', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-004', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-004', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-004', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260913-005', 16, 1, CAST(39000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260915-001', 3, 1, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260915-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260920-001', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260920-001', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260920-002', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260920-002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260920-003', 5, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'MH260920-003', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00001', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00001', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00001', 46, 1, CAST(47000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00002', 8, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00003', 48, 1, CAST(33000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00003', 54, 1, CAST(35000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00003', 57, 2, CAST(37000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00004', 25, 1, CAST(41000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00004', 50, 2, CAST(42000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00004', 57, 2, CAST(37000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00005', 33, 2, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00005', 52, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00006', 24, 1, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00006', 50, 2, CAST(42000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00006', 61, 1, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00007', 24, 2, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00007', 47, 1, CAST(28000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00007', 63, 2, CAST(34000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00008', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00008', 56, 2, CAST(42000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00009', 10, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00009', 32, 1, CAST(35000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00009', 52, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00010', 40, 1, CAST(35000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00010', 47, 2, CAST(28000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00011', 7, 2, CAST(41000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00012', 40, 1, CAST(35000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00012', 41, 1, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00013', 50, 1, CAST(42000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00014', 13, 1, CAST(38000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00014', 47, 1, CAST(28000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00015', 2, 1, CAST(41000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00015', 21, 1, CAST(31000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00015', 26, 1, CAST(46000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00016', 38, 1, CAST(25000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00017', 56, 2, CAST(42000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00018', 12, 1, CAST(41000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00018', 41, 2, CAST(25000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00019', 28, 1, CAST(44000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00019', 31, 2, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00020', 47, 1, CAST(28000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00021', 1, 1, CAST(36000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00021', 4, 2, CAST(34000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00021', 12, 2, CAST(41000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00022', 24, 2, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00022', 28, 2, CAST(44000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00022', 54, 1, CAST(35000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00023', 49, 2, CAST(37000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00024', 39, 1, CAST(30000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00024', 43, 2, CAST(35000.00 AS Decimal(18, 2)), 0)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00025', 18, 2, CAST(36000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00025', 42, 1, CAST(30000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00025', 61, 2, CAST(38000.00 AS Decimal(18, 2)), 1)
GO
INSERT [dbo].[order_items] ([order_id], [variant_id], [quantity], [unit_price], [is_completed]) VALUES (N'ORD00026', 3, 2, CAST(29000.00 AS Decimal(18, 2)), 0)
GO

-- -------------------------------------------------------------
-- Data for table: orders (154 rows)
-- -------------------------------------------------------------
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260731-001', N'CB005', 15, CAST(N'2026-07-31T21:18:12.1329209' AS DateTime2), CAST(66000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'A', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260731-002', N'CB005', 15, CAST(N'2026-07-31T21:18:45.9398778' AS DateTime2), CAST(138000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Completed', N'A', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260731-003', N'CB005', 15, CAST(N'2026-07-31T21:19:15.3774265' AS DateTime2), CAST(61000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'B', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-001', N'CB004', 6, CAST(N'2026-08-06T17:36:42.5838902' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'232321232', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-002', N'CB004', 6, CAST(N'2026-08-06T17:36:53.2730726' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'232321232', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-003', N'CB004', 6, CAST(N'2026-08-06T17:49:37.8495362' AS DateTime2), CAST(149000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'12343234', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-004', N'CB004', 6, CAST(N'2026-08-06T17:49:42.4235793' AS DateTime2), CAST(149000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'12343234', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-005', N'CB004', 6, CAST(N'2026-08-06T17:51:07.6591644' AS DateTime2), CAST(148000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'12313124', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-006', N'CB004', 6, CAST(N'2026-08-06T17:51:15.3496513' AS DateTime2), CAST(148000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'haskhlashfk', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-007', N'CB004', 6, CAST(N'2026-08-06T21:03:39.5285753' AS DateTime2), CAST(446000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'1233234', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-008', N'CB004', 6, CAST(N'2026-08-06T21:04:00.0288201' AS DateTime2), CAST(163000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'2345432345', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-009', N'CB004', 6, CAST(N'2026-08-06T21:04:26.2794870' AS DateTime2), CAST(3101000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'142', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-010', N'CB004', 6, CAST(N'2026-08-06T21:04:39.3828705' AS DateTime2), CAST(3101000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'142', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-011', N'CB004', 6, CAST(N'2026-08-06T21:06:00.4071944' AS DateTime2), CAST(87000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'1', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-012', N'CB004', 6, CAST(N'2026-08-06T21:44:46.1019910' AS DateTime2), CAST(126000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'1', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260806-013', N'CB004', 6, CAST(N'2026-08-06T22:39:58.3304797' AS DateTime2), CAST(90000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'asadasd', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-001', N'CB004', 6, CAST(N'2026-08-07T00:09:54.0752485' AS DateTime2), CAST(179000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'q', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-002', N'CB004', 6, CAST(N'2026-08-07T00:15:03.6102556' AS DateTime2), CAST(87000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'dsafasffa', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-003', N'CB004', 6, CAST(N'2026-08-07T13:06:00.7949096' AS DateTime2), CAST(119000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'12', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-004', N'CB004', 6, CAST(N'2026-08-07T13:32:41.6405634' AS DateTime2), CAST(108000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'gg', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-005', N'CB004', 6, CAST(N'2026-08-07T13:53:22.8889366' AS DateTime2), CAST(282000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'sdasdsasdf', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-006', N'CB004', 6, CAST(N'2026-08-07T13:54:38.9207655' AS DateTime2), CAST(282000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'sdasdsasdf', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-007', N'CB004', 6, CAST(N'2026-08-07T14:06:14.2438160' AS DateTime2), CAST(1242000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-008', N'CB004', 6, CAST(N'2026-08-07T14:06:17.1457452' AS DateTime2), CAST(1242000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-009', N'CB004', 6, CAST(N'2026-08-07T14:20:32.2825537' AS DateTime2), CAST(387000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-010', N'CB004', 6, CAST(N'2026-08-07T14:20:47.1723248' AS DateTime2), CAST(551000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260807-011', N'CB004', 6, CAST(N'2026-08-07T14:21:43.7836090' AS DateTime2), CAST(952000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260809-001', N'CB004', 6, CAST(N'2026-08-09T13:45:14.2509145' AS DateTime2), CAST(237000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260809-002', N'CB004', 6, CAST(N'2026-08-09T13:46:57.1884735' AS DateTime2), CAST(180000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260809-003', N'CB004', 6, CAST(N'2026-08-09T14:49:32.0872300' AS DateTime2), CAST(145000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260809-004', N'CB004', 6, CAST(N'2026-08-09T16:11:51.2401515' AS DateTime2), CAST(263000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260809-005', N'CB004', 6, CAST(N'2026-08-09T16:16:50.5650417' AS DateTime2), CAST(591000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-001', N'CB004', 6, CAST(N'2026-08-12T00:34:23.3939786' AS DateTime2), CAST(197000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-002', N'CB004', 6, CAST(N'2026-08-12T11:30:01.6901240' AS DateTime2), CAST(190000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-003', N'CB004', 5, CAST(N'2026-08-12T11:56:56.3116292' AS DateTime2), CAST(513000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-004', N'CB004', 6, CAST(N'2026-08-12T12:40:54.5977536' AS DateTime2), CAST(180000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-005', N'CB004', 6, CAST(N'2026-08-12T05:58:15.6249410' AS DateTime2), CAST(309000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'gg', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-006', N'CB004', 6, CAST(N'2026-08-12T06:04:18.3667284' AS DateTime2), CAST(233000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260812-007', N'CB004', 6, CAST(N'2026-08-12T06:04:56.9051441' AS DateTime2), CAST(76000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-001', N'CB004', 6, CAST(N'2026-08-13T15:15:54.2346628' AS DateTime2), CAST(76000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-002', N'CB004', 6, CAST(N'2026-08-13T15:47:30.4428945' AS DateTime2), CAST(179000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-003', N'CB004', 6, CAST(N'2026-08-13T17:02:44.2784057' AS DateTime2), CAST(179000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-004', N'CB004', 6, CAST(N'2026-08-13T17:03:13.3806281' AS DateTime2), CAST(179000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-005', N'CB004', 6, CAST(N'2026-08-13T17:09:33.7806288' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-006', N'CB004', 6, CAST(N'2026-08-13T17:37:43.5127648' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-007', N'CB004', 6, CAST(N'2026-08-13T17:38:12.9046582' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-008', N'CB004', 6, CAST(N'2026-08-13T17:38:37.5419717' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-009', N'CB004', 6, CAST(N'2026-08-13T17:39:01.7162316' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-010', N'CB004', 6, CAST(N'2026-08-13T17:39:19.3652797' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-011', N'CB004', 6, CAST(N'2026-08-13T17:42:17.6938909' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-012', N'CB004', 6, CAST(N'2026-08-13T17:43:06.1318225' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-013', N'CB004', 6, CAST(N'2026-08-13T17:53:11.1199427' AS DateTime2), CAST(194000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-014', N'CB004', 6, CAST(N'2026-08-13T17:53:36.5741640' AS DateTime2), CAST(646000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-015', N'CB004', 6, CAST(N'2026-08-13T18:17:38.9675566' AS DateTime2), CAST(646000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-016', N'CB004', 6, CAST(N'2026-08-13T18:20:06.0641649' AS DateTime2), CAST(636000.00 AS Decimal(18, 2)), N'Cash (Đưa:636,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-017', N'CB004', 5, CAST(N'2026-08-13T18:31:14.7129504' AS DateTime2), CAST(144000.00 AS Decimal(18, 2)), N'Cash (Đưa:144,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-018', N'CB004', 5, CAST(N'2026-08-13T20:59:09.0983950' AS DateTime2), CAST(201000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-019', N'CB004', 5, CAST(N'2026-08-13T20:59:40.5392127' AS DateTime2), CAST(201000.00 AS Decimal(18, 2)), N'Cash (Đưa:201,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260813-020', N'CB004', 5, CAST(N'2026-08-13T20:59:52.9001094' AS DateTime2), CAST(145000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-001', N'CB004', 6, CAST(N'2026-08-14T11:16:49.9246489' AS DateTime2), CAST(145000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-002', N'CB004', 6, CAST(N'2026-08-14T11:18:52.1893798' AS DateTime2), CAST(398000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-003', N'CB004', 6, CAST(N'2026-08-14T11:22:51.2934543' AS DateTime2), CAST(108000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-004', N'CB004', 6, CAST(N'2026-08-14T21:29:53.3518687' AS DateTime2), CAST(179000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-005', N'CB004', 6, CAST(N'2026-08-14T21:30:33.3791449' AS DateTime2), CAST(179000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-006', N'CB004', 6, CAST(N'2026-08-14T21:32:00.7235565' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-007', N'CB004', 6, CAST(N'2026-08-14T21:37:16.8994563' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-008', N'CB004', 6, CAST(N'2026-08-14T21:57:17.8054718' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'CK SePay (066ITC1262262004) - Đã nhận:25,000đ', N'TransferSuccessPending', N'Completed', N'Me', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-009', N'CB004', 6, CAST(N'2026-08-14T20:05:52.6161902' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-010', N'CB004', 6, CAST(N'2026-08-14T20:07:19.8613639' AS DateTime2), CAST(31000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Me', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-011', N'CB004', 6, CAST(N'2026-08-14T20:16:21.7442082' AS DateTime2), CAST(36000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260814-012', N'CB004', 6, CAST(N'2026-08-14T20:20:45.4723157' AS DateTime2), CAST(36000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260815-001', N'CB004', 6, CAST(N'2026-08-15T00:29:27.5684840' AS DateTime2), CAST(36000.00 AS Decimal(18, 2)), NULL, N'Unpaid', N'Cancelled / Refunded', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260815-002', N'CB004', 6, CAST(N'2026-08-15T14:01:57.7039046' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260815-003', N'CB004', 6, CAST(N'2026-08-15T14:03:53.7727119' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260815-004', N'CB004', 6, CAST(N'2026-08-15T16:11:05.7589368' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Cancelled / Refunded', N'Me', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260815-005', N'CB004', 6, CAST(N'2026-08-15T23:37:44.0102331' AS DateTime2), CAST(67000.00 AS Decimal(18, 2)), N'Cash (Đưa:70,000đ - Thừa:3,000đ)', N'Paid', N'Cancelled / Refunded', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260815-006', N'CB004', 6, CAST(N'2026-08-15T23:41:53.8580766' AS DateTime2), CAST(68000.00 AS Decimal(18, 2)), N'Cash (Đưa:70,000đ - Thừa:2,000đ)', N'Paid', N'Missing Ingredients', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-001', N'CB004', 6, CAST(N'2026-08-16T00:04:41.1114328' AS DateTime2), CAST(29000.00 AS Decimal(18, 2)), N'Cash (Đưa:29,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-002', N'CB004', 6, CAST(N'2026-08-16T07:23:10.9504671' AS DateTime2), CAST(61000.00 AS Decimal(18, 2)), N'Cash (Đưa:59,000đ - Thừa:0đ) + Thu thêm Tiền mặt (2,000đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-003', N'CB004', 6, CAST(N'2026-08-16T07:57:25.2392404' AS DateTime2), CAST(67000.00 AS Decimal(18, 2)), N'Cash (Đưa:70,000đ - Thừa:3,000đ)', N'Partially Refunded', N'Partially Refunded', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-004', N'CB004', 6, CAST(N'2026-08-16T08:16:25.8518457' AS DateTime2), CAST(55000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Cancelled / Refunded', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-005', N'CB004', 6, CAST(N'2026-08-16T08:22:00.7027012' AS DateTime2), CAST(67000.00 AS Decimal(18, 2)), N'Cash (Đưa:70,000đ - Thừa:3,000đ)', N'Paid', N'Missing Ingredients', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-006', N'CB004', 6, CAST(N'2026-08-16T08:55:36.8343384' AS DateTime2), CAST(155000.00 AS Decimal(18, 2)), N'Cash (Đưa:200,000đ - Thừa:45,000đ)', N'Cancelled', N'Cancelled / Refunded', N'5 ae sieu nhan', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-007', N'CB004', 6, CAST(N'2026-08-16T08:34:40.6328306' AS DateTime2), CAST(99000.00 AS Decimal(18, 2)), N'Cash (Đưa:500,000đ - Thừa:401,000đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-008', N'CB004', 6, CAST(N'2026-08-16T08:36:02.8593731' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), N'Cash (Đưa:59,000đ - Thừa:0đ)', N'Paid', N'Missing Ingredients', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-009', N'CB004', 6, CAST(N'2026-08-16T08:36:15.9772466' AS DateTime2), CAST(137000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260816-010', N'CB004', 6, CAST(N'2026-08-16T08:37:27.1652908' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), N'Cash (Đưa:59,000đ - Thừa:0đ)', N'Paid', N'Missing Ingredients', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-001', N'CB004', 6, CAST(N'2026-08-27T07:35:56.9530769' AS DateTime2), CAST(124000.00 AS Decimal(18, 2)), N'Cash (Đưa:124,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-002', N'CB004', 6, CAST(N'2026-08-27T07:36:05.5640531' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-003', N'CB004', 6, CAST(N'2026-08-27T07:51:40.4336916' AS DateTime2), CAST(64000.00 AS Decimal(18, 2)), N'Cash (Đưa:100,000đ - Thừa:36,000đ)', N'Cancelled', N'Cancelled / Refunded', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-004', N'CB004', 6, CAST(N'2026-08-27T08:36:45.4332899' AS DateTime2), CAST(228000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-005', N'CB004', 6, CAST(N'2026-08-27T08:38:02.5571461' AS DateTime2), CAST(71000.00 AS Decimal(18, 2)), N'Cash (Đưa:71,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-006', N'CB004', 6, CAST(N'2026-08-27T13:02:50.1640725' AS DateTime2), CAST(29000.00 AS Decimal(18, 2)), N'Cash (Đưa:29,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260827-007', N'CB004', 6, CAST(N'2026-08-27T13:07:40.1995081' AS DateTime2), CAST(304000.00 AS Decimal(18, 2)), N'Cash (Đưa:304,000đ - Thừa:0đ)', N'Paid', N'Delivered', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-001', N'CB004', 6, CAST(N'2026-09-03T10:40:50.8673687' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Cancelled / Refunded', N'1', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-002', N'CB004', 6, CAST(N'2026-09-03T10:42:09.6206721' AS DateTime2), CAST(29000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Missing Ingredients', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-003', N'CB004', 6, CAST(N'2026-09-03T10:50:00.8855266' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer 1 phần (Xác nhận thủ công #MBB-110046 - Đã nhận:12,500đ)', N'PartiallyPaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-004', N'CB004', 6, CAST(N'2026-09-03T11:19:06.3203876' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-005', N'CB004', 6, CAST(N'2026-09-03T11:29:07.4778304' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'Split (CK:12,500đ + TM:12,500đ (Đưa:12,500đ - Thừa:0đ))', N'Paid', N'Missing Ingredients', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-006', N'CB004', 6, CAST(N'2026-09-03T11:39:23.7432701' AS DateTime2), CAST(50000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-007', N'CB004', 6, CAST(N'2026-09-03T11:40:24.6494966' AS DateTime2), CAST(50000.00 AS Decimal(18, 2)), N'Bank Transfer 1 phần (Xác nhận thủ công #MBB-114028 - Đã nhận:25,000đ)', N'PartiallyPaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-008', N'CB004', 6, CAST(N'2026-09-03T11:51:58.2432399' AS DateTime2), CAST(281000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Khách lẻ', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260903-009', N'CB004', 6, CAST(N'2026-09-03T20:49:19.2325349' AS DateTime2), CAST(6000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Completed', N'Bàn 03 - Khách lẻ', N'Bàn 03', N'Khách lẻ', CAST(56000.00 AS Decimal(18, 2)), CAST(20000.00 AS Decimal(18, 2)), CAST(30000.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260906-001', N'CB004', 6, CAST(N'2026-09-06T13:16:11.9979133' AS DateTime2), CAST(76000.00 AS Decimal(18, 2)), N'Cash (Đưa:76,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(126000.00 AS Decimal(18, 2)), CAST(20000.00 AS Decimal(18, 2)), CAST(30000.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260906-002', N'CB004', 6, CAST(N'2026-09-06T13:34:06.2580548' AS DateTime2), CAST(40000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Cancelled', N'Cancelled / Refunded', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(59000.00 AS Decimal(18, 2)), CAST(10000.00 AS Decimal(18, 2)), CAST(9000.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260906-003', N'CB004', 6, CAST(N'2026-09-06T14:15:38.4334576' AS DateTime2), CAST(29000.00 AS Decimal(18, 2)), N'Cash (Đưa:29,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(29000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260906-004', N'CB004', 6, CAST(N'2026-09-06T14:19:12.7261455' AS DateTime2), CAST(89000.00 AS Decimal(18, 2)), N'Cash (Đưa:89,000đ - Thừa:0đ)', N'Paid', N'Completed', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(89000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260909-001', N'CB004', 6, CAST(N'2026-09-09T20:36:10.9356649' AS DateTime2), CAST(406400.00 AS Decimal(18, 2)), N'Cash (Đưa:406,400đ - Thừa:0đ)', N'Paid', N'Delivered', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(508000.00 AS Decimal(18, 2)), CAST(101600.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 4, N'SUMMER20')
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260909-002', N'CB004', 6, CAST(N'2026-09-09T20:51:13.9687740' AS DateTime2), CAST(137000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(137000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260909-003', N'CB004', 6, CAST(N'2026-09-09T20:51:52.3342760' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-001', N'CB004', 6, CAST(N'2026-09-12T07:10:45.5381657' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-002', N'CB004', 6, CAST(N'2026-09-12T07:12:49.9096227' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-003', N'CB004', 6, CAST(N'2026-09-12T08:09:29.8208589' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-004', N'CB004', 6, CAST(N'2026-09-12T08:21:20.1421027' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-005', N'CB004', 6, CAST(N'2026-09-12T08:21:44.4433082' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-006', N'CB004', 6, CAST(N'2026-09-12T08:29:32.1342208' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-007', N'CB004', 6, CAST(N'2026-09-12T08:29:34.8652809' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Unpaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-008', N'CB004', 6, CAST(N'2026-09-12T08:35:45.8428278' AS DateTime2), CAST(112000.00 AS Decimal(18, 2)), N'Split', N'PartiallyPaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(112000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260912-009', N'CB004', 6, CAST(N'2026-09-12T16:52:42.7303377' AS DateTime2), CAST(131000.00 AS Decimal(18, 2)), N'Split', N'PartiallyPaid', N'Waiting', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(131000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260913-001', N'CB004', 6, CAST(N'2026-09-13T19:49:46.5119963' AS DateTime2), CAST(176800.00 AS Decimal(18, 2)), N'Cash (Đưa:176,800đ - Thừa:0đ)', N'Paid', N'Completed', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(221000.00 AS Decimal(18, 2)), CAST(44200.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 4, N'SUMMER20')
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260913-002', N'CB004', 6, CAST(N'2026-09-13T21:00:13.0685446' AS DateTime2), CAST(65000.00 AS Decimal(18, 2)), N'Split (CK:15,000đ + TM:50,000đ)', N'Paid', N'Waiting for Brewing', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(65000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260913-003', N'CB004', 6, CAST(N'2026-09-13T21:02:37.2249769' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), N'Split (CK:9,000đ + TM:50,000đ)', N'Paid', N'Waiting for Brewing', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(59000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260913-004', N'CB004', 6, CAST(N'2026-09-13T21:23:54.4897980' AS DateTime2), CAST(76800.00 AS Decimal(18, 2)), N'Cash (Đưa:100,000đ - Thừa:23,200đ)', N'Paid', N'Completed', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(96000.00 AS Decimal(18, 2)), CAST(19200.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), 4, N'SUMMER20')
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260913-005', N'CB004', 6, CAST(N'2026-09-13T21:25:22.5072510' AS DateTime2), CAST(39000.00 AS Decimal(18, 2)), N'Cash (Đưa:50,000đ - Thừa:11,000đ)', N'Paid', N'Waiting for Brewing', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(39000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260915-001', N'CB005', 15, CAST(N'2026-09-15T21:35:33.9683870' AS DateTime2), CAST(59000.00 AS Decimal(18, 2)), N'Split (CK:36,780đ + TM:22,220đ)', N'Paid', N'Waiting for Brewing', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(59000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260920-001', N'CB004', 6, CAST(N'2026-09-20T21:02:38.4649508' AS DateTime2), CAST(88000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Waiting for Brewing', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(88000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260920-002', N'CB005', 16, CAST(N'2026-09-20T21:17:48.3770851' AS DateTime2), CAST(88000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Brewing in Progress', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(88000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'MH260920-003', N'CB005', 16, CAST(N'2026-09-20T21:18:08.2140336' AS DateTime2), CAST(62000.00 AS Decimal(18, 2)), N'Bank Transfer', N'Paid', N'Waiting for Brewing', N'Mang về - Khách lẻ', N'Mang về', N'Khách lẻ', CAST(62000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'', NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00001', N'CB004', 6, CAST(N'2026-07-23T17:00:00.0000000' AS DateTime2), CAST(114000.00 AS Decimal(18, 2)), N'Momo', N'Unpaid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00002', N'CB004', 6, CAST(N'2026-07-21T13:00:00.0000000' AS DateTime2), CAST(30000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00003', N'CB004', 6, CAST(N'2026-07-22T07:00:00.0000000' AS DateTime2), CAST(142000.00 AS Decimal(18, 2)), N'BankTransfer', N'Unpaid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00004', N'CB004', 6, CAST(N'2026-07-25T14:00:00.0000000' AS DateTime2), CAST(199000.00 AS Decimal(18, 2)), N'Cash', N'Unpaid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00005', N'CB004', 6, CAST(N'2026-07-26T15:00:00.0000000' AS DateTime2), CAST(126000.00 AS Decimal(18, 2)), N'BankTransfer', N'Unpaid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00006', N'CB004', 6, CAST(N'2026-07-24T20:00:00.0000000' AS DateTime2), CAST(158000.00 AS Decimal(18, 2)), N'Momo', N'Paid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00007', N'CB004', 6, CAST(N'2026-07-26T18:00:00.0000000' AS DateTime2), CAST(168000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00008', N'CB004', 6, CAST(N'2026-07-25T20:00:00.0000000' AS DateTime2), CAST(109000.00 AS Decimal(18, 2)), N'Momo', N'Unpaid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00009', N'CB004', 6, CAST(N'2026-07-25T21:00:00.0000000' AS DateTime2), CAST(116000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00010', N'CB004', 6, CAST(N'2026-07-23T06:00:00.0000000' AS DateTime2), CAST(91000.00 AS Decimal(18, 2)), N'Momo', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00011', N'CB004', 6, CAST(N'2026-07-21T16:00:00.0000000' AS DateTime2), CAST(82000.00 AS Decimal(18, 2)), N'Momo', N'Paid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00012', N'CB005', 16, CAST(N'2026-07-28T20:00:00.0000000' AS DateTime2), CAST(60000.00 AS Decimal(18, 2)), N'CreditCard', N'Paid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00013', N'CB005', 15, CAST(N'2026-07-28T18:00:00.0000000' AS DateTime2), CAST(42000.00 AS Decimal(18, 2)), N'CreditCard', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00014', N'CB005', 16, CAST(N'2026-07-30T14:00:00.0000000' AS DateTime2), CAST(66000.00 AS Decimal(18, 2)), N'CreditCard', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00015', N'CB005', 12, CAST(N'2026-07-24T21:00:00.0000000' AS DateTime2), CAST(118000.00 AS Decimal(18, 2)), N'CreditCard', N'Unpaid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00016', N'CB005', 16, CAST(N'2026-07-27T21:00:00.0000000' AS DateTime2), CAST(25000.00 AS Decimal(18, 2)), N'CreditCard', N'Paid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00017', N'CB005', 12, CAST(N'2026-07-30T08:00:00.0000000' AS DateTime2), CAST(84000.00 AS Decimal(18, 2)), N'Momo', N'Paid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00018', N'CB005', 16, CAST(N'2026-07-29T06:00:00.0000000' AS DateTime2), CAST(91000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00019', N'CB005', 12, CAST(N'2026-07-27T06:00:00.0000000' AS DateTime2), CAST(104000.00 AS Decimal(18, 2)), N'Momo', N'Unpaid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00020', N'CB005', 16, CAST(N'2026-07-27T07:00:00.0000000' AS DateTime2), CAST(28000.00 AS Decimal(18, 2)), N'Momo', N'Paid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00021', N'CB005', 16, CAST(N'2026-07-27T10:00:00.0000000' AS DateTime2), CAST(186000.00 AS Decimal(18, 2)), N'Momo', N'Paid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00022', N'CB005', 16, CAST(N'2026-07-28T19:00:00.0000000' AS DateTime2), CAST(195000.00 AS Decimal(18, 2)), N'Momo', N'Unpaid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00023', N'CB005', 16, CAST(N'2026-07-30T07:00:00.0000000' AS DateTime2), CAST(74000.00 AS Decimal(18, 2)), N'CreditCard', N'Paid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00024', N'CB005', 16, CAST(N'2026-07-30T12:00:00.0000000' AS DateTime2), CAST(100000.00 AS Decimal(18, 2)), N'Momo', N'Unpaid', N'Brewing', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00025', N'CB005', 15, CAST(N'2026-07-25T21:00:00.0000000' AS DateTime2), CAST(178000.00 AS Decimal(18, 2)), N'Cash', N'Paid', N'Done', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO
INSERT [dbo].[orders] ([order_id], [branch_id], [cashier_id], [created_at], [total_amount], [payment_method], [payment_status], [brewing_status], [recipient_name], [table_number], [customer_name], [subtotal_amount], [discount_amount], [trade_discount_amount], [order_notes], [bank_transaction_code], [refund_amount], [refund_reason], [refund_method], [refunded_at], [cash_amount], [bank_amount], [voucher_id], [voucher_code]) VALUES (N'ORD00026', N'CB005', 12, CAST(N'2026-07-21T08:00:00.0000000' AS DateTime2), CAST(58000.00 AS Decimal(18, 2)), N'CreditCard', N'Unpaid', N'Waiting', NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL, CAST(0.00 AS Decimal(18, 2)), NULL, NULL, NULL, CAST(0.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, NULL)
GO

-- -------------------------------------------------------------
-- Data for table: payments (92 rows)
-- -------------------------------------------------------------
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-BNK-MH260913-002-140208-896', N'MH260913-002', N'CB004', 6, N'Payment', N'BankTransfer', CAST(15000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-140208-192', NULL, NULL, N'Split (CK:15,000đ + TM:50,000đ)', CAST(N'2026-09-13T14:02:08.4737554' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-BNK-MH260913-003-140305-123', N'MH260913-003', N'CB004', 6, N'Payment', N'BankTransfer', CAST(9000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-140305-895', NULL, NULL, N'Split (CK:9,000đ + TM:50,000đ)', CAST(N'2026-09-13T14:03:05.4816541' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-BNK-MH260915-001-143549-964', N'MH260915-001', N'CB005', 15, N'Payment', N'BankTransfer', CAST(36780.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-143549-638', NULL, NULL, N'Split (CK:36,780đ + TM:22,220đ)', CAST(N'2026-09-15T14:35:49.5547333' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-BNK-MH260920-001-140244-172', N'MH260920-001', N'CB004', 6, N'Payment', N'BankTransfer', CAST(88000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-140244-716', NULL, NULL, N'Bank Transfer (Xác nhận thủ công #QR-MANUAL-140244-716 - Đã nhận:88,000đ)', CAST(N'2026-09-20T14:02:44.0009129' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-BNK-MH260920-002-141752-306', N'MH260920-002', N'CB005', 16, N'Payment', N'BankTransfer', CAST(88000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-141752-165', NULL, NULL, N'Bank Transfer (Xác nhận thủ công #QR-MANUAL-141752-165 - Đã nhận:88,000đ)', CAST(N'2026-09-20T14:17:52.7364888' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-BNK-MH260920-003-141811-704', N'MH260920-003', N'CB005', 16, N'Payment', N'BankTransfer', CAST(62000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-141811-950', NULL, NULL, N'Bank Transfer (Xác nhận thủ công #QR-MANUAL-141811-950 - Đã nhận:62,000đ)', CAST(N'2026-09-20T14:18:11.9371682' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-CSH-MH260912-008-013546-194', N'MH260912-008', N'CB004', 6, N'Payment', N'Cash', CAST(100000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Thanh toán tiền mặt 1 phần trước khi chuyển khoản nốt', CAST(N'2026-09-12T01:35:46.0396881' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-CSH-MH260912-009-095242-176', N'MH260912-009', N'CB004', 6, N'Payment', N'Cash', CAST(100000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(100000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Thanh toán tiền mặt 1 phần trước khi chuyển khoản nốt', CAST(N'2026-09-12T09:52:42.8213040' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-CSH-MH260913-002-140013-447', N'MH260913-002', N'CB004', 6, N'Payment', N'Cash', CAST(50000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(50000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Thanh toán tiền mặt 1 phần trước khi chuyển khoản nốt', CAST(N'2026-09-13T14:00:13.3716669' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-CSH-MH260913-003-140237-400', N'MH260913-003', N'CB004', 6, N'Payment', N'Cash', CAST(50000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(50000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Thanh toán tiền mặt 1 phần trước khi chuyển khoản nốt', CAST(N'2026-09-13T14:02:37.2471761' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-CSH-MH260915-001-143534-318', N'MH260915-001', N'CB005', 15, N'Payment', N'Cash', CAST(22220.00 AS Decimal(18, 2)), N'Success', NULL, CAST(22220.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Thanh toán tiền mặt 1 phần trước khi chuyển khoản nốt', CAST(N'2026-09-15T14:35:34.1016317' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260731-002-LEG', N'MH260731-002', N'CB005', 15, N'Payment', N'Cash', CAST(138000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(138000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-31T21:18:45.9398778' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260731-003-LEG', N'MH260731-003', N'CB005', 15, N'Payment', N'BankTransfer', CAST(61000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-31T21:19:15.3774265' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260806-006-LEG', N'MH260806-006', N'CB004', 6, N'Payment', N'BankTransfer', CAST(148000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-06T17:51:15.3496513' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260806-012-LEG', N'MH260806-012', N'CB004', 6, N'Payment', N'BankTransfer', CAST(126000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-06T21:44:46.1019910' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260807-001-LEG', N'MH260807-001', N'CB004', 6, N'Payment', N'BankTransfer', CAST(179000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-07T00:09:54.0752485' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260807-002-LEG', N'MH260807-002', N'CB004', 6, N'Payment', N'BankTransfer', CAST(87000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-07T00:15:03.6102556' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260807-003-LEG', N'MH260807-003', N'CB004', 6, N'Payment', N'BankTransfer', CAST(119000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-07T13:06:00.7949096' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260807-004-LEG', N'MH260807-004', N'CB004', 6, N'Payment', N'BankTransfer', CAST(108000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-07T13:32:41.6405634' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260807-008-LEG', N'MH260807-008', N'CB004', 6, N'Payment', N'BankTransfer', CAST(1242000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-07T14:06:17.1457452' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260807-011-LEG', N'MH260807-011', N'CB004', 6, N'Payment', N'Cash', CAST(952000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(952000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-07T14:21:43.7836090' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260809-001-LEG', N'MH260809-001', N'CB004', 6, N'Payment', N'BankTransfer', CAST(237000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-09T13:45:14.2509145' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260809-002-LEG', N'MH260809-002', N'CB004', 6, N'Payment', N'Cash', CAST(180000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(180000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-09T13:46:57.1884735' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260809-004-LEG', N'MH260809-004', N'CB004', 6, N'Payment', N'BankTransfer', CAST(263000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-09T16:11:51.2401515' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260809-005-LEG', N'MH260809-005', N'CB004', 6, N'Payment', N'Cash', CAST(591000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(591000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-09T16:16:50.5650417' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260812-003-LEG', N'MH260812-003', N'CB004', 5, N'Payment', N'BankTransfer', CAST(513000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-12T11:56:56.3116292' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260812-004-LEG', N'MH260812-004', N'CB004', 6, N'Payment', N'BankTransfer', CAST(180000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-12T12:40:54.5977536' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260812-005-LEG', N'MH260812-005', N'CB004', 6, N'Payment', N'BankTransfer', CAST(309000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-12T05:58:15.6249410' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260812-006-LEG', N'MH260812-006', N'CB004', 6, N'Payment', N'Cash', CAST(233000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(233000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-12T06:04:18.3667284' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260813-016-LEG', N'MH260813-016', N'CB004', 6, N'Payment', N'Cash', CAST(636000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(636000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-13T18:20:06.0641649' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260813-017-LEG', N'MH260813-017', N'CB004', 5, N'Payment', N'Cash', CAST(144000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(144000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-13T18:31:14.7129504' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260813-019-LEG', N'MH260813-019', N'CB004', 5, N'Payment', N'Cash', CAST(201000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(201000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-13T20:59:40.5392127' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260814-002-LEG', N'MH260814-002', N'CB004', 6, N'Payment', N'BankTransfer', CAST(398000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-14T11:18:52.1893798' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260814-009-LEG', N'MH260814-009', N'CB004', 6, N'Payment', N'BankTransfer', CAST(25000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-14T20:05:52.6161902' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260814-010-LEG', N'MH260814-010', N'CB004', 6, N'Payment', N'BankTransfer', CAST(31000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-14T20:07:19.8613639' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260815-003-LEG', N'MH260815-003', N'CB004', 6, N'Payment', N'BankTransfer', CAST(25000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-15T14:03:53.7727119' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260815-004-LEG', N'MH260815-004', N'CB004', 6, N'Payment', N'BankTransfer', CAST(25000.00 AS Decimal(18, 2)), N'Success', N'MBB-166549', NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-15T16:11:05.7589368' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260815-005-LEG', N'MH260815-005', N'CB004', 6, N'Payment', N'Cash', CAST(67000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(67000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-15T23:37:44.0102331' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260815-006-LEG', N'MH260815-006', N'CB004', 6, N'Payment', N'Cash', CAST(68000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(68000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-15T23:41:53.8580766' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-001-LEG', N'MH260816-001', N'CB004', 6, N'Payment', N'Cash', CAST(29000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(29000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T00:04:41.1114328' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-002-LEG', N'MH260816-002', N'CB004', 6, N'Payment', N'Cash', CAST(61000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(61000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T07:23:10.9504671' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-005-LEG', N'MH260816-005', N'CB004', 6, N'Payment', N'Cash', CAST(67000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(67000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T08:22:00.7027012' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-007-LEG', N'MH260816-007', N'CB004', 6, N'Payment', N'Cash', CAST(99000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(99000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T08:34:40.6328306' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-008-LEG', N'MH260816-008', N'CB004', 6, N'Payment', N'Cash', CAST(59000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(59000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T08:36:02.8593731' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-009-LEG', N'MH260816-009', N'CB004', 6, N'Payment', N'BankTransfer', CAST(137000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-083624-142', NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T08:36:15.9772466' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260816-010-LEG', N'MH260816-010', N'CB004', 6, N'Payment', N'Cash', CAST(59000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(59000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-16T08:37:27.1652908' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260827-001-LEG', N'MH260827-001', N'CB004', 6, N'Payment', N'Cash', CAST(124000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(124000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-27T07:35:56.9530769' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260827-002-LEG', N'MH260827-002', N'CB004', 6, N'Payment', N'BankTransfer', CAST(59000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-073609-205', NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-27T07:36:05.5640531' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260827-004-LEG', N'MH260827-004', N'CB004', 6, N'Payment', N'BankTransfer', CAST(228000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-083648-392', NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-27T08:36:45.4332899' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260827-005-LEG', N'MH260827-005', N'CB004', 6, N'Payment', N'Cash', CAST(71000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(71000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-27T08:38:02.5571461' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260827-006-LEG', N'MH260827-006', N'CB004', 6, N'Payment', N'Cash', CAST(29000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(29000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-27T13:02:50.1640725' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260827-007-LEG', N'MH260827-007', N'CB004', 6, N'Payment', N'Cash', CAST(304000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(304000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-08-27T13:07:40.1995081' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-001-BNK', N'MH260903-001', N'CB004', 6, N'Payment', N'BankTransfer', CAST(59000.00 AS Decimal(18, 2)), N'Success', N'MBB-104134', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T10:40:50.8673687' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-002-BNK', N'MH260903-002', N'CB004', 6, N'Payment', N'BankTransfer', CAST(29000.00 AS Decimal(18, 2)), N'Success', N'MBB-104237', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T10:42:09.6206721' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-003-BNK', N'MH260903-003', N'CB004', 6, N'Payment', N'BankTransfer', CAST(12500.00 AS Decimal(18, 2)), N'Success', N'MBB-110046', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T10:50:00.8855266' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-005-BNK', N'MH260903-005', N'CB004', 6, N'Payment', N'BankTransfer', CAST(12500.00 AS Decimal(18, 2)), N'Success', N'MBB-113107', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T11:29:07.4778304' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-005-CSH', N'MH260903-005', N'CB004', 6, N'Payment', N'Cash', CAST(12500.00 AS Decimal(18, 2)), N'Success', NULL, CAST(12500.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« trÆ°á»ng cash_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T11:29:07.4778304' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-007-BNK', N'MH260903-007', N'CB004', 6, N'Payment', N'BankTransfer', CAST(25000.00 AS Decimal(18, 2)), N'Success', N'MBB-114028', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T11:40:24.6494966' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260903-009-BNK', N'MH260903-009', N'CB004', 6, N'Payment', N'BankTransfer', CAST(6000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-134928-614', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-03T20:49:19.2325349' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260906-001-CSH', N'MH260906-001', N'CB004', 6, N'Payment', N'Cash', CAST(76000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(76000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« trÆ°á»ng cash_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-06T13:16:11.9979133' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260906-002-BNK', N'MH260906-002', N'CB004', 6, N'Payment', N'BankTransfer', CAST(40000.00 AS Decimal(18, 2)), N'Success', N'QR-MANUAL-063446-843', NULL, NULL, N'Di trÃº tá»« trÆ°á»ng bank_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-06T13:34:06.2580548' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260906-003-CSH', N'MH260906-003', N'CB004', 6, N'Payment', N'Cash', CAST(29000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(29000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« trÆ°á»ng cash_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-06T14:15:38.4334576' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260906-004-CSH', N'MH260906-004', N'CB004', 6, N'Payment', N'Cash', CAST(89000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(89000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« trÆ°á»ng cash_amount cÅ© cá»§a Ä‘Æ¡n hÃ ng', CAST(N'2026-09-06T14:19:12.7261455' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260909-001-133611-787', N'MH260909-001', N'CB004', 6, N'Payment', N'Cash', CAST(406400.00 AS Decimal(18, 2)), N'Success', NULL, CAST(406400.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, CAST(N'2026-09-09T13:36:11.1846354' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260913-001-124946-458', N'MH260913-001', N'CB004', 6, N'Payment', N'Cash', CAST(176800.00 AS Decimal(18, 2)), N'Success', NULL, CAST(176800.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), NULL, CAST(N'2026-09-13T12:49:46.6135236' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260913-004-142354-750', N'MH260913-004', N'CB004', 6, N'Payment', N'Cash', CAST(76800.00 AS Decimal(18, 2)), N'Success', NULL, CAST(100000.00 AS Decimal(18, 2)), CAST(23200.00 AS Decimal(18, 2)), NULL, CAST(N'2026-09-13T14:23:54.5634448' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-MH260913-005-142522-323', N'MH260913-005', N'CB004', 6, N'Payment', N'Cash', CAST(39000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(50000.00 AS Decimal(18, 2)), CAST(11000.00 AS Decimal(18, 2)), NULL, CAST(N'2026-09-13T14:25:22.5480546' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00002-LEG', N'ORD00002', N'CB004', 6, N'Payment', N'Cash', CAST(30000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(30000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-21T13:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00006-LEG', N'ORD00006', N'CB004', 6, N'Payment', N'Momo', CAST(158000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-24T20:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00007-LEG', N'ORD00007', N'CB004', 6, N'Payment', N'Cash', CAST(168000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(168000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-26T18:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00009-LEG', N'ORD00009', N'CB004', 6, N'Payment', N'Cash', CAST(116000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(116000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-25T21:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00010-LEG', N'ORD00010', N'CB004', 6, N'Payment', N'Momo', CAST(91000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-23T06:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00011-LEG', N'ORD00011', N'CB004', 6, N'Payment', N'Momo', CAST(82000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-21T16:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00012-LEG', N'ORD00012', N'CB005', 16, N'Payment', N'CreditCard', CAST(60000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-28T20:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00013-LEG', N'ORD00013', N'CB005', 15, N'Payment', N'CreditCard', CAST(42000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-28T18:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00014-LEG', N'ORD00014', N'CB005', 16, N'Payment', N'CreditCard', CAST(66000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-30T14:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00016-LEG', N'ORD00016', N'CB005', 16, N'Payment', N'CreditCard', CAST(25000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-27T21:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00017-LEG', N'ORD00017', N'CB005', 12, N'Payment', N'Momo', CAST(84000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-30T08:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00018-LEG', N'ORD00018', N'CB005', 16, N'Payment', N'Cash', CAST(91000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(91000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-29T06:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00020-LEG', N'ORD00020', N'CB005', 16, N'Payment', N'Momo', CAST(28000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-27T07:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00021-LEG', N'ORD00021', N'CB005', 16, N'Payment', N'Momo', CAST(186000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-27T10:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00023-LEG', N'ORD00023', N'CB005', 16, N'Payment', N'CreditCard', CAST(74000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-30T07:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'PAY-ORD00025-LEG', N'ORD00025', N'CB005', 15, N'Payment', N'Cash', CAST(178000.00 AS Decimal(18, 2)), N'Success', NULL, CAST(178000.00 AS Decimal(18, 2)), CAST(0.00 AS Decimal(18, 2)), N'Di trÃº tá»« Ä‘Æ¡n hÃ ng Ä‘Ã£ hoÃ n táº¥t cÅ© (legacy Paid)', CAST(N'2026-07-25T21:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260815-001-01', N'MH260815-001', N'CB004', 6, N'Refund', N'Cash', CAST(36000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Hết nguyên liệu pha chế', CAST(N'2026-08-15T18:02:28.7839398' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260815-004-01', N'MH260815-004', N'CB004', 6, N'Refund', N'Cash', CAST(25000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Hết nguyên liệu pha chế', CAST(N'2026-08-15T16:16:48.3988937' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260815-005-01', N'MH260815-005', N'CB004', 6, N'Refund', N'Cash', CAST(36000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Hết nguyên liệu pha chế', CAST(N'2026-08-15T23:39:37.9167023' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260816-003-01', N'MH260816-003', N'CB004', 6, N'Refund', N'Cash', CAST(36000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Hết nguyên liệu pha chế', CAST(N'2026-08-16T08:06:26.5797164' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260816-004-01', N'MH260816-004', N'CB004', 6, N'Refund', N'Cash', CAST(55000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Pha nhầm món / Lỗi chất lượng', CAST(N'2026-08-16T08:54:42.7739735' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260816-006-01', N'MH260816-006', N'CB004', 6, N'Refund', N'Cash', CAST(155000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Khách đổi ý / Hủy đơn trước khi pha', CAST(N'2026-08-16T08:58:22.6755846' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260827-003-01', N'MH260827-003', N'CB004', 6, N'Refund', N'Cash', CAST(64000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Khách đổi ý / Hủy đơn trước khi pha', CAST(N'2026-08-27T08:11:31.8398518' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260903-001-01', N'MH260903-001', N'CB004', 6, N'Refund', N'Cash', CAST(59000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Hết nguyên liệu pha chế', CAST(N'2026-09-06T06:42:25.3583803' AS DateTime2))
GO
INSERT [dbo].[payments] ([payment_id], [order_id], [branch_id], [cashier_id], [payment_type], [payment_method], [amount], [status], [transaction_code], [customer_cash], [change_amount], [notes], [created_at]) VALUES (N'REF-MH260906-002-01', N'MH260906-002', N'CB004', 6, N'Refund', N'Cash', CAST(40000.00 AS Decimal(18, 2)), N'Success', NULL, NULL, NULL, N'Hết nguyên liệu pha chế', CAST(N'2026-09-06T06:46:42.1372798' AS DateTime2))
GO

-- -------------------------------------------------------------
-- Data for table: product_categories (6 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[product_categories] ON
GO
INSERT [dbo].[product_categories] ([category_id], [category_name], [description], [status]) VALUES (1, N'Cà phê', N'Các loại cà phê pha máy và pha phin', N'Active')
GO
INSERT [dbo].[product_categories] ([category_id], [category_name], [description], [status]) VALUES (2, N'Trà', N'Trà trái cây và trà truyền thống', N'Active')
GO
INSERT [dbo].[product_categories] ([category_id], [category_name], [description], [status]) VALUES (3, N'Nước ép & Sinh tố', N'Nước ép trái cây tươi và sinh tố', N'Active')
GO
INSERT [dbo].[product_categories] ([category_id], [category_name], [description], [status]) VALUES (4, N'Đá xay', N'Các loại đồ uống đá xay', N'Active')
GO
INSERT [dbo].[product_categories] ([category_id], [category_name], [description], [status]) VALUES (5, N'Bánh ngọt', N'Bánh ngọt và tráng miệng', N'Active')
GO
INSERT [dbo].[product_categories] ([category_id], [category_name], [description], [status]) VALUES (6, N'Topping', N'Topping thêm cho đồ uống', N'Active')
GO
SET IDENTITY_INSERT [dbo].[product_categories] OFF
GO

-- -------------------------------------------------------------
-- Data for table: product_variants (64 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[product_variants] ON
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (1, 1, N'S', CAST(36000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (2, 1, N'M', CAST(41000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (3, 2, N'S', CAST(29000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (4, 2, N'M', CAST(34000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (5, 3, N'S', CAST(31000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (6, 3, N'M', CAST(36000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (7, 3, N'L', CAST(41000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (8, 4, N'S', CAST(30000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (9, 4, N'M', CAST(35000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (10, 5, N'S', CAST(31000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (11, 5, N'M', CAST(36000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (12, 5, N'L', CAST(41000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (13, 6, N'S', CAST(38000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (14, 6, N'M', CAST(43000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (15, 6, N'L', CAST(48000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (16, 7, N'S', CAST(39000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (17, 7, N'M', CAST(44000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (18, 8, N'S', CAST(36000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (19, 8, N'M', CAST(41000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (20, 8, N'L', CAST(46000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (21, 9, N'S', CAST(31000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (22, 9, N'M', CAST(36000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (23, 9, N'L', CAST(41000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (24, 10, N'S', CAST(36000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (25, 10, N'M', CAST(41000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (26, 10, N'L', CAST(46000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (27, 11, N'S', CAST(39000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (28, 11, N'M', CAST(44000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (29, 11, N'L', CAST(49000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (30, 12, N'S', CAST(25000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (31, 12, N'M', CAST(30000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (32, 12, N'L', CAST(35000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (33, 13, N'S', CAST(38000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (34, 13, N'M', CAST(43000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (35, 14, N'S', CAST(34000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (36, 14, N'M', CAST(39000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (37, 14, N'L', CAST(44000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (38, 15, N'S', CAST(25000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (39, 15, N'M', CAST(30000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (40, 15, N'L', CAST(35000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (41, 16, N'S', CAST(25000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (42, 16, N'M', CAST(30000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (43, 16, N'L', CAST(35000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (44, 17, N'S', CAST(37000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (45, 17, N'M', CAST(42000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (46, 17, N'L', CAST(47000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (47, 18, N'S', CAST(28000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (48, 18, N'M', CAST(33000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (49, 19, N'S', CAST(37000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (50, 19, N'M', CAST(42000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (51, 19, N'L', CAST(47000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (52, 20, N'S', CAST(25000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (53, 20, N'M', CAST(30000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (54, 20, N'L', CAST(35000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (55, 21, N'S', CAST(37000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (56, 21, N'M', CAST(42000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (57, 22, N'S', CAST(37000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (58, 22, N'M', CAST(42000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (59, 22, N'L', CAST(47000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (60, 23, N'S', CAST(33000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (61, 23, N'M', CAST(38000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (62, 24, N'S', CAST(29000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (63, 24, N'M', CAST(34000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[product_variants] ([variant_id], [product_id], [size_variant], [selling_price]) VALUES (1002, 1, N'L', CAST(100000.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[product_variants] OFF
GO

-- -------------------------------------------------------------
-- Data for table: recipes (92 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[recipes] ON
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (1, 1, 1, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (2, 1, 3, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (3, 2, 1, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (4, 2, 3, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (5, 1002, 1, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (6, 1002, 3, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (7, 3, 1, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (8, 3, 2, CAST(28.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (9, 4, 1, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (10, 4, 2, CAST(35.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (11, 5, 1, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (12, 5, 2, CAST(32.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (13, 5, 4, CAST(64.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (14, 6, 1, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (15, 6, 2, CAST(40.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (16, 6, 4, CAST(80.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (17, 7, 1, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (18, 7, 2, CAST(50.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (19, 7, 4, CAST(100.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (20, 8, 1, CAST(14.40 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (21, 9, 1, CAST(18.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (22, 10, 7, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (23, 10, 6, CAST(32.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (24, 10, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (25, 11, 7, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (26, 11, 6, CAST(40.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (27, 11, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (28, 12, 7, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (29, 12, 6, CAST(50.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (30, 12, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (31, 13, 7, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (32, 13, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (33, 14, 7, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (34, 14, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (35, 15, 7, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (36, 15, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (37, 16, 7, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (38, 16, 5, CAST(32.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (39, 16, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (40, 17, 7, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (41, 17, 5, CAST(40.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (42, 17, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (43, 18, 7, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (44, 18, 3, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (45, 19, 7, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (46, 19, 3, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (47, 20, 7, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (48, 20, 3, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (49, 21, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (50, 22, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (51, 23, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (52, 24, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (53, 25, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (54, 26, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (55, 27, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (56, 28, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (57, 29, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (58, 30, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (59, 31, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (60, 32, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (61, 33, 1, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (62, 33, 4, CAST(64.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (63, 33, 2, CAST(24.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (64, 33, 3, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (65, 34, 1, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (66, 34, 4, CAST(80.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (67, 34, 2, CAST(30.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (68, 34, 3, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (69, 35, 4, CAST(80.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (70, 35, 8, CAST(24.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (71, 35, 3, CAST(12.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (72, 36, 4, CAST(100.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (73, 36, 8, CAST(30.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (74, 36, 3, CAST(15.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (75, 37, 4, CAST(125.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (76, 37, 8, CAST(37.50 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (77, 37, 3, CAST(18.80 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (78, 38, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (79, 39, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (80, 40, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (81, 41, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (82, 42, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (83, 43, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (84, 55, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (85, 56, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (86, 57, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (87, 58, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (88, 59, 3, CAST(25.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (89, 60, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (90, 61, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (91, 62, 3, CAST(16.00 AS Decimal(18, 2)), NULL)
GO
INSERT [dbo].[recipes] ([recipe_id], [variant_id], [material_id], [quantity], [branch_id]) VALUES (92, 63, 3, CAST(20.00 AS Decimal(18, 2)), NULL)
GO
SET IDENTITY_INSERT [dbo].[recipes] OFF
GO

-- -------------------------------------------------------------
-- Data for table: shift_change_requests (20 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[shift_change_requests] ON
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1, 10, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), N'Submitted', N'CB005', 9)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2, 4, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), N'Submitted', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (3, 11, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), N'Rejected', N'CB005', 9)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (4, 3, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), N'Submitted', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (5, 3, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (6, 6, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), N'Submitted', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (7, 6, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (8, 7, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), N'Submitted', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (9, 3, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), N'Submitted', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (10, 15, N'Đổi sang ca sáng', N'Lý do cá nhân', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1002, 4, N'1234564323456434567543', N'123453443444543', CAST(N'2026-08-09T14:45:38.8636837' AS DateTime2), N'Submitted', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1003, 6, N'333', N'333', CAST(N'2026-08-09T16:23:36.1386443' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1004, 6, N'ca 1 => ca 3', N'14214123', CAST(N'2026-08-09T16:24:01.8374495' AS DateTime2), N'Submitted', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1005, 6, N'12345', N'12345432345', CAST(N'2026-08-10T03:29:09.9660049' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1006, 6, N'ca toi', N'doi ca', CAST(N'2026-08-12T00:32:01.4677520' AS DateTime2), N'Submitted', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (1007, 7, N'doi sang ca trua', N'met', CAST(N'2026-08-12T11:40:25.8108266' AS DateTime2), N'Submitted', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2005, 6, N'tu choi', N'tu choi', CAST(N'2026-08-14T11:43:38.5427651' AS DateTime2), N'Approved', N'CB004', 2)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2006, 13, N'ca1', N'ca1', CAST(N'2026-08-15T19:04:08.2825642' AS DateTime2), N'Approved', N'CB005', 9)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2007, 6, N'ca đổi', N'đổi ca', CAST(N'2026-09-15T14:16:50.5422133' AS DateTime2), N'Canceled', NULL, NULL)
GO
INSERT [dbo].[shift_change_requests] ([request_id], [requesting_employee_id], [aspiration], [reason], [submitted_at], [status], [approved_branch_id], [approved_manager_id]) VALUES (2008, 6, N'ca đổi', N'đổi ca', CAST(N'2026-09-15T14:17:10.3721700' AS DateTime2), N'Approved', N'CB004', 2)
GO
SET IDENTITY_INSERT [dbo].[shift_change_requests] OFF
GO

-- -------------------------------------------------------------
-- Data for table: vouchers (2 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[vouchers] ON
GO
INSERT [dbo].[vouchers] ([voucher_id], [voucher_code], [branch_id], [discount_percent], [quantity], [used_count], [start_date], [end_date], [description], [is_active], [created_by], [created_at]) VALUES (3, N'WELCOME10', N'CB004', CAST(10.00 AS Decimal(5, 2)), 100, 0, CAST(N'2026-09-08T18:41:07.2466667' AS DateTime2), CAST(N'2026-10-09T18:41:07.2466667' AS DateTime2), N'Giảm 10% cho khách hàng mới', 0, NULL, CAST(N'2026-09-09T18:41:07.2466667' AS DateTime2))
GO
INSERT [dbo].[vouchers] ([voucher_id], [voucher_code], [branch_id], [discount_percent], [quantity], [used_count], [start_date], [end_date], [description], [is_active], [created_by], [created_at]) VALUES (4, N'SUMMER20', N'CB004', CAST(20.00 AS Decimal(5, 2)), 50, 4, CAST(N'2026-09-08T18:41:07.2500000' AS DateTime2), CAST(N'2026-09-24T18:41:07.2500000' AS DateTime2), N'Khuyến mãi mùa hè giảm 20% toàn bộ đơn hàng', 1, NULL, CAST(N'2026-09-09T18:41:07.2500000' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[vouchers] OFF
GO

-- -------------------------------------------------------------
-- Data for table: warehouse_receipt_items (11 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[warehouse_receipt_items] ON
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (1, 1, 1, CAST(500.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(75000000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (2, 2, 2, CAST(100.00 AS Decimal(18, 2)), CAST(35000.00 AS Decimal(18, 2)), CAST(3500000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (3, 3, 1, CAST(300.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(45000000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (4, 4, 1, CAST(13.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(1950000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (5, 5, 1, CAST(500.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(75000000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (6, 6, 5, CAST(60.00 AS Decimal(18, 2)), CAST(85000.00 AS Decimal(18, 2)), CAST(5100000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (7, 7, 8, CAST(57.00 AS Decimal(18, 2)), CAST(180000.00 AS Decimal(18, 2)), CAST(10260000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (8, 8, 2, CAST(1.00 AS Decimal(18, 2)), CAST(35000.00 AS Decimal(18, 2)), CAST(35000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (1006, 1006, 6, CAST(100.00 AS Decimal(18, 2)), CAST(65000.00 AS Decimal(18, 2)), CAST(6500000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (1007, 1007, 1, CAST(100.00 AS Decimal(18, 2)), CAST(150000.00 AS Decimal(18, 2)), CAST(15000000.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[warehouse_receipt_items] ([receipt_item_id], [receipt_id], [material_id], [quantity], [unit_price], [amount]) VALUES (1008, 1008, 9, CAST(68.00 AS Decimal(18, 2)), CAST(20000.00 AS Decimal(18, 2)), CAST(1360000.00 AS Decimal(18, 2)))
GO
SET IDENTITY_INSERT [dbo].[warehouse_receipt_items] OFF
GO

-- -------------------------------------------------------------
-- Data for table: warehouse_receipts (11 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[warehouse_receipts] ON
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (1, N'PN-001', CAST(N'2026-06-12T09:30:00.0000000' AS DateTime2), N'Trung Nguyên', CAST(75000000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Nguyễn Hữu B', N'Nguyễn Văn A', N'0397321999', N'Nguyễn Hữu B')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (2, N'PN-002', CAST(N'2026-06-12T14:15:00.0000000' AS DateTime2), N'Vinamilk', CAST(3500000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Nguyễn Hữu B', N'Nguyễn Thị C', N'0905123456', N'Nguyễn Hữu B')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (3, N'PN-003', CAST(N'2026-06-15T10:00:00.0000000' AS DateTime2), N'Trung Nguyên', CAST(45000000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Nguyễn Hữu B', N'Nguyễn Văn A', N'0397321999', N'Nguyễn Hữu B')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (4, N'PN-004', CAST(N'2026-08-09T14:27:49.7070211' AS DateTime2), N'Trung Nguyên', CAST(1950000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'qwerty', N'12345671234', N'easfasg')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (5, N'PN-005', CAST(N'2026-08-09T15:12:19.3185454' AS DateTime2), N'Trung Nguyên', CAST(75000000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'123789', N'123478934567892', N'Warehouse Manager')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (6, N'PN-006', CAST(N'2026-08-10T16:48:56.5090132' AS DateTime2), N'Sen Việt', CAST(5100000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'12', N'1213', N'Warehouse Manager')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (7, N'PN-007', CAST(N'2026-08-10T16:49:42.1938858' AS DateTime2), N'Monin', CAST(10260000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'12', N'12', N'Warehouse Manager')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (8, N'PN-008', CAST(N'2026-08-12T22:20:02.4999062' AS DateTime2), N'Vinamilk', CAST(35000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'A', N'', N'B')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (1006, N'PN-009', CAST(N'2026-08-14T11:40:40.2862746' AS DateTime2), N'Kronos', CAST(6500000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'123131312', N'13213261551', N'Warehouse Manager')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (1007, N'PN-1007', CAST(N'2026-09-15T14:08:40.2117912' AS DateTime2), N'Trung Nguyên', CAST(15000000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'0', N'0904592741', N'Warehouse Manager')
GO
INSERT [dbo].[warehouse_receipts] ([receipt_id], [receipt_code], [import_date], [supplier], [total_amount], [status], [created_by], [deliverer_name], [deliverer_phone], [receiver_name]) VALUES (1008, N'PN-1008', CAST(N'2026-09-19T07:51:15.6427372' AS DateTime2), N'Quân', CAST(1360000.00 AS Decimal(18, 2)), N'Đã nhập kho', N'Warehouse Manager', N'Temp', N'', N'Warehouse Manager')
GO
SET IDENTITY_INSERT [dbo].[warehouse_receipts] OFF
GO

-- -------------------------------------------------------------
-- Data for table: weekly_roster_grids (330 rows)
-- -------------------------------------------------------------
SET IDENTITY_INSERT [dbo].[weekly_roster_grids] ON
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1, N'CB004', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2, N'CB004', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (3, N'CB004', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (4, N'CB004', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (5, N'CB004', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (6, N'CB004', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (7, N'CB004', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 2, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (8, N'CB004', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (9, N'CB004', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (10, N'CB004', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (11, N'CB004', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (12, N'CB004', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (13, N'CB004', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (14, N'CB004', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (15, N'CB004', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (16, N'CB004', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (17, N'CB004', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (18, N'CB004', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (19, N'CB004', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (20, N'CB004', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (21, N'CB004', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (22, N'CB004', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 1, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (23, N'CB004', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (24, N'CB004', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (25, N'CB004', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 1, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (26, N'CB004', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (27, N'CB004', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (28, N'CB004', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 2, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (29, N'CB004', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (30, N'CB004', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (31, N'CB004', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 1, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (32, N'CB004', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (33, N'CB004', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (34, N'CB004', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 1, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (35, N'CB004', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (36, N'CB004', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (37, N'CB004', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (38, N'CB004', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 2, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (39, N'CB004', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (40, N'CB004', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (41, N'CB005', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 1, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (42, N'CB005', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 3, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (43, N'CB005', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 3, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (44, N'CB005', CAST(N'2026-07-20T00:00:00.0000000' AS DateTime2), 3, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (45, N'CB005', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 1, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (46, N'CB005', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 3, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (47, N'CB005', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 2, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (48, N'CB005', CAST(N'2026-07-21T00:00:00.0000000' AS DateTime2), 1, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (49, N'CB005', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 2, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (50, N'CB005', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 3, 13)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (51, N'CB005', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 2, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (52, N'CB005', CAST(N'2026-07-22T00:00:00.0000000' AS DateTime2), 1, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (53, N'CB005', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 3, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (54, N'CB005', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 3, 16)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (55, N'CB005', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 1, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (56, N'CB005', CAST(N'2026-07-23T00:00:00.0000000' AS DateTime2), 3, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (57, N'CB005', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 2, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (58, N'CB005', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 1, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (59, N'CB005', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 1, 16)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (60, N'CB005', CAST(N'2026-07-24T00:00:00.0000000' AS DateTime2), 2, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (61, N'CB005', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 3, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (62, N'CB005', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 2, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (63, N'CB005', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 2, 13)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (64, N'CB005', CAST(N'2026-07-25T00:00:00.0000000' AS DateTime2), 3, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (65, N'CB005', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 3, 13)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (66, N'CB005', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 1, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (67, N'CB005', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 3, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (68, N'CB005', CAST(N'2026-07-26T00:00:00.0000000' AS DateTime2), 3, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (69, N'CB005', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 3, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (70, N'CB005', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 2, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (71, N'CB005', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 1, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (72, N'CB005', CAST(N'2026-07-27T00:00:00.0000000' AS DateTime2), 1, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (73, N'CB005', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 2, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (74, N'CB005', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 1, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (75, N'CB005', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 3, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (76, N'CB005', CAST(N'2026-07-28T00:00:00.0000000' AS DateTime2), 3, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (77, N'CB005', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 3, 10)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (78, N'CB005', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 2, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (79, N'CB005', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 1, 13)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (80, N'CB005', CAST(N'2026-07-29T00:00:00.0000000' AS DateTime2), 3, 12)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (81, N'CB004', CAST(N'2026-07-31T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (82, N'CB004', CAST(N'2026-07-31T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (83, N'CB004', CAST(N'2026-07-31T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (84, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (85, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (86, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (87, N'CB004', CAST(N'2026-08-14T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (89, N'CB004', CAST(N'2026-08-14T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (90, N'CB004', CAST(N'2026-08-21T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (92, N'CB004', CAST(N'2026-08-21T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (93, N'CB004', CAST(N'2026-08-28T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (95, N'CB004', CAST(N'2026-08-28T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (99, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (100, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (101, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (102, N'CB004', CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (103, N'CB004', CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (104, N'CB004', CAST(N'2026-08-10T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (107, N'CB004', CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (108, N'CB004', CAST(N'2026-08-24T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (110, N'CB004', CAST(N'2026-08-24T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (111, N'CB004', CAST(N'2026-08-31T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (112, N'CB004', CAST(N'2026-08-31T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (113, N'CB004', CAST(N'2026-08-31T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (116, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (117, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (118, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (119, N'CB004', CAST(N'2026-08-03T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1002, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1003, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1004, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1005, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1007, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1008, N'CB004', CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1010, N'CB004', CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1011, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1013, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1015, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1016, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1017, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1018, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1019, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1020, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1021, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1023, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1024, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1025, N'CB004', CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1027, N'CB004', CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1028, N'CB004', CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1029, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1031, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1032, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1033, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1034, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1035, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1036, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1039, N'CB004', CAST(N'2026-08-06T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1040, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1041, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1042, N'CB004', CAST(N'2026-08-07T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1043, N'CB004', CAST(N'2026-08-14T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1045, N'CB004', CAST(N'2026-08-14T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1046, N'CB004', CAST(N'2026-08-21T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1048, N'CB004', CAST(N'2026-08-21T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1049, N'CB004', CAST(N'2026-08-28T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1051, N'CB004', CAST(N'2026-08-28T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1055, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1056, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1057, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1060, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1063, N'CB004', CAST(N'2026-08-23T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1066, N'CB004', CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1069, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 2, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1070, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1071, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1072, N'CB004', CAST(N'2026-08-09T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1075, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1076, N'CB004', CAST(N'2026-08-23T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1078, N'CB004', CAST(N'2026-08-23T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1079, N'CB004', CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1081, N'CB004', CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1082, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1084, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1085, N'CB004', CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1086, N'CB004', CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1087, N'CB004', CAST(N'2026-08-11T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1088, N'CB004', CAST(N'2026-08-18T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1090, N'CB004', CAST(N'2026-08-18T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1091, N'CB004', CAST(N'2026-08-25T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1093, N'CB004', CAST(N'2026-08-25T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1094, N'CB004', CAST(N'2026-09-01T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1095, N'CB004', CAST(N'2026-09-01T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1096, N'CB004', CAST(N'2026-09-01T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1097, N'CB004', CAST(N'2026-09-08T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1098, N'CB004', CAST(N'2026-09-08T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1099, N'CB004', CAST(N'2026-09-08T00:00:00.0000000' AS DateTime2), 3, 8)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1100, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1101, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1102, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1104, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1105, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1106, N'CB004', CAST(N'2026-08-19T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1108, N'CB004', CAST(N'2026-08-19T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1111, N'CB004', CAST(N'2026-08-19T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1112, N'CB004', CAST(N'2026-08-26T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1114, N'CB004', CAST(N'2026-08-26T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1117, N'CB004', CAST(N'2026-08-26T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1118, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1119, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1120, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1122, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1123, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1124, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1125, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1126, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1128, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1129, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1135, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1136, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1137, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1138, N'CB004', CAST(N'2026-08-12T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1139, N'CB004', CAST(N'2026-08-19T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1140, N'CB004', CAST(N'2026-08-19T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1142, N'CB004', CAST(N'2026-08-19T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1143, N'CB004', CAST(N'2026-08-26T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1144, N'CB004', CAST(N'2026-08-26T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1146, N'CB004', CAST(N'2026-08-26T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1147, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1148, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1149, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1150, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1151, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1152, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1153, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (1154, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2085, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2086, N'CB004', CAST(N'2026-08-13T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2087, N'CB004', CAST(N'2026-08-20T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2088, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2089, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2091, N'CB004', CAST(N'2026-08-14T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2092, N'CB004', CAST(N'2026-08-14T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2094, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2095, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2096, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2098, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2099, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2100, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2101, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2102, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2104, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2105, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2106, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2107, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2108, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2110, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2111, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2112, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2113, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2114, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2116, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2117, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2118, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2119, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2120, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 2, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2122, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2123, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 3, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2124, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2125, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2126, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2129, N'CB005', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 1, 16)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2130, N'CB005', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 1, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2131, N'CB005', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 1, 13)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2132, N'CB005', CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), 1, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2133, N'CB005', CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), 1, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2134, N'CB005', CAST(N'2026-08-17T00:00:00.0000000' AS DateTime2), 1, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2136, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2138, N'CB004', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2139, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2141, N'CB004', CAST(N'2026-08-22T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2142, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2144, N'CB004', CAST(N'2026-08-29T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2145, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2147, N'CB004', CAST(N'2026-09-05T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2148, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2150, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 3, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2151, N'CB005', CAST(N'2026-08-15T00:00:00.0000000' AS DateTime2), 3, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2152, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2153, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 1, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2154, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2157, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2158, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2159, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 2, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2160, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2161, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 3, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2162, N'CB004', CAST(N'2026-08-23T00:00:00.0000000' AS DateTime2), 2, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2163, N'CB004', CAST(N'2026-08-23T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2164, N'CB004', CAST(N'2026-08-23T00:00:00.0000000' AS DateTime2), 3, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2165, N'CB004', CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), 2, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2166, N'CB004', CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2167, N'CB004', CAST(N'2026-08-30T00:00:00.0000000' AS DateTime2), 3, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2168, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 2, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2169, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2170, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 3, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2171, N'CB004', CAST(N'2026-08-16T00:00:00.0000000' AS DateTime2), 3, 5)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2172, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2173, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2174, N'CB004', CAST(N'2026-08-27T00:00:00.0000000' AS DateTime2), 2, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2175, N'CB004', CAST(N'2026-09-02T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2176, N'CB004', CAST(N'2026-09-09T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2177, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 2, 1005)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2178, N'CB004', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2179, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2180, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 1, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2181, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2182, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2183, N'CB004', CAST(N'2026-09-03T00:00:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2184, N'CB004', CAST(N'2026-09-10T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2185, N'CB004', CAST(N'2026-09-10T00:00:00.0000000' AS DateTime2), 1, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2186, N'CB004', CAST(N'2026-09-10T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2187, N'CB004', CAST(N'2026-09-10T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2188, N'CB004', CAST(N'2026-09-10T00:00:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2190, N'CB004', CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), 1, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2191, N'CB004', CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2192, N'CB004', CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2193, N'CB004', CAST(N'2026-09-17T00:00:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2194, N'CB004', CAST(N'2026-09-24T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2195, N'CB004', CAST(N'2026-09-24T00:00:00.0000000' AS DateTime2), 1, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2196, N'CB004', CAST(N'2026-09-24T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2197, N'CB004', CAST(N'2026-09-24T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2198, N'CB004', CAST(N'2026-09-24T00:00:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2199, N'CB004', CAST(N'2026-10-01T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2200, N'CB004', CAST(N'2026-10-01T00:00:00.0000000' AS DateTime2), 1, 1004)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2201, N'CB004', CAST(N'2026-10-01T00:00:00.0000000' AS DateTime2), 1, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2202, N'CB004', CAST(N'2026-10-01T00:00:00.0000000' AS DateTime2), 1, 4)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2203, N'CB004', CAST(N'2026-10-01T00:00:00.0000000' AS DateTime2), 1, 3)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (2204, N'CB004', CAST(N'2026-09-04T00:00:00.0000000' AS DateTime2), 2, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (3175, N'CB005', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 3, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (3176, N'CB005', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 3, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (3177, N'CB005', CAST(N'2026-09-06T00:00:00.0000000' AS DateTime2), 3, 11)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (3178, N'CB004', CAST(N'2026-09-12T00:00:00.0000000' AS DateTime2), 1, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (4178, N'CB004', CAST(N'2026-09-13T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (5178, N'CB004', CAST(N'2026-09-14T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (5179, N'CB004', CAST(N'2026-09-15T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (5180, N'CB005', CAST(N'2026-09-15T00:00:00.0000000' AS DateTime2), 3, 15)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (6179, N'CB004', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 6)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (6180, N'CB004', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 7)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (6181, N'CB005', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 16)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (6182, N'CB005', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 14)
GO
INSERT [dbo].[weekly_roster_grids] ([roster_id], [branch_id], [assignment_date], [shift_id], [employee_id]) VALUES (6183, N'CB005', CAST(N'2026-09-20T00:00:00.0000000' AS DateTime2), 3, 10)
GO
SET IDENTITY_INSERT [dbo].[weekly_roster_grids] OFF
GO

/*******************************************************************************
   PART 3: FOREIGN KEY CONSTRAINTS
*******************************************************************************/
ALTER TABLE [dbo].[attendance_logs]  WITH CHECK ADD  CONSTRAINT [FK_attendance_logs_employees_employee_id] FOREIGN KEY([employee_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[attendance_logs] CHECK CONSTRAINT [FK_attendance_logs_employees_employee_id]
GO
ALTER TABLE [dbo].[attendance_logs]  WITH CHECK ADD  CONSTRAINT [FK_attendance_logs_weekly_roster_grids_roster_id] FOREIGN KEY([roster_id])
REFERENCES [dbo].[weekly_roster_grids] ([roster_id])
GO
ALTER TABLE [dbo].[attendance_logs] CHECK CONSTRAINT [FK_attendance_logs_weekly_roster_grids_roster_id]
GO
ALTER TABLE [dbo].[branch_inventories]  WITH CHECK ADD  CONSTRAINT [FK_branch_inventories_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[branch_inventories] CHECK CONSTRAINT [FK_branch_inventories_branches_branch_id]
GO
ALTER TABLE [dbo].[branch_inventories]  WITH CHECK ADD  CONSTRAINT [FK_branch_inventories_materials_material_id] FOREIGN KEY([material_id])
REFERENCES [dbo].[materials] ([material_id])
GO
ALTER TABLE [dbo].[branch_inventories] CHECK CONSTRAINT [FK_branch_inventories_materials_material_id]
GO
ALTER TABLE [dbo].[branch_managers]  WITH CHECK ADD  CONSTRAINT [FK_branch_managers_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[branch_managers] CHECK CONSTRAINT [FK_branch_managers_branches_branch_id]
GO
ALTER TABLE [dbo].[branch_managers]  WITH CHECK ADD  CONSTRAINT [FK_branch_managers_employees_manager_id] FOREIGN KEY([manager_id])
REFERENCES [dbo].[employees] ([employee_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[branch_managers] CHECK CONSTRAINT [FK_branch_managers_employees_manager_id]
GO
ALTER TABLE [dbo].[branch_menus]  WITH CHECK ADD  CONSTRAINT [FK_branch_menus_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
GO
ALTER TABLE [dbo].[branch_menus] CHECK CONSTRAINT [FK_branch_menus_branches_branch_id]
GO
ALTER TABLE [dbo].[branch_supply_request_items]  WITH CHECK ADD  CONSTRAINT [FK_branch_supply_request_items_branch_supply_requests_request_id] FOREIGN KEY([request_id])
REFERENCES [dbo].[branch_supply_requests] ([request_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[branch_supply_request_items] CHECK CONSTRAINT [FK_branch_supply_request_items_branch_supply_requests_request_id]
GO
ALTER TABLE [dbo].[branch_supply_request_items]  WITH CHECK ADD  CONSTRAINT [FK_branch_supply_request_items_materials_material_id] FOREIGN KEY([material_id])
REFERENCES [dbo].[materials] ([material_id])
GO
ALTER TABLE [dbo].[branch_supply_request_items] CHECK CONSTRAINT [FK_branch_supply_request_items_materials_material_id]
GO
ALTER TABLE [dbo].[branch_supply_requests]  WITH CHECK ADD  CONSTRAINT [FK_branch_supply_requests_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[branch_supply_requests] CHECK CONSTRAINT [FK_branch_supply_requests_branches_branch_id]
GO
ALTER TABLE [dbo].[cash_handovers]  WITH CHECK ADD  CONSTRAINT [FK_cash_handovers_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
GO
ALTER TABLE [dbo].[cash_handovers] CHECK CONSTRAINT [FK_cash_handovers_branches_branch_id]
GO
ALTER TABLE [dbo].[cash_handovers]  WITH CHECK ADD  CONSTRAINT [FK_cash_handovers_employees_incoming_cashier_id] FOREIGN KEY([incoming_cashier_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[cash_handovers] CHECK CONSTRAINT [FK_cash_handovers_employees_incoming_cashier_id]
GO
ALTER TABLE [dbo].[cash_handovers]  WITH CHECK ADD  CONSTRAINT [FK_cash_handovers_employees_outgoing_cashier_id] FOREIGN KEY([outgoing_cashier_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[cash_handovers] CHECK CONSTRAINT [FK_cash_handovers_employees_outgoing_cashier_id]
GO
ALTER TABLE [dbo].[cash_handovers]  WITH CHECK ADD  CONSTRAINT [FK_cash_handovers_fixed_shifts_shift_id] FOREIGN KEY([shift_id])
REFERENCES [dbo].[fixed_shifts] ([shift_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cash_handovers] CHECK CONSTRAINT [FK_cash_handovers_fixed_shifts_shift_id]
GO
ALTER TABLE [dbo].[employees]  WITH CHECK ADD  CONSTRAINT [FK_employees_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
GO
ALTER TABLE [dbo].[employees] CHECK CONSTRAINT [FK_employees_branches_branch_id]
GO
ALTER TABLE [dbo].[leave_applications]  WITH CHECK ADD  CONSTRAINT [FK_leave_applications_branch_managers_approved_branch_id_approved_manager_id] FOREIGN KEY([approved_branch_id], [approved_manager_id])
REFERENCES [dbo].[branch_managers] ([branch_id], [manager_id])
GO
ALTER TABLE [dbo].[leave_applications] CHECK CONSTRAINT [FK_leave_applications_branch_managers_approved_branch_id_approved_manager_id]
GO
ALTER TABLE [dbo].[leave_applications]  WITH CHECK ADD  CONSTRAINT [FK_leave_applications_employees_employee_id] FOREIGN KEY([employee_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[leave_applications] CHECK CONSTRAINT [FK_leave_applications_employees_employee_id]
GO
ALTER TABLE [dbo].[master_products]  WITH CHECK ADD  CONSTRAINT [FK_master_products_product_categories_category_id] FOREIGN KEY([category_id])
REFERENCES [dbo].[product_categories] ([category_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[master_products] CHECK CONSTRAINT [FK_master_products_product_categories_category_id]
GO
ALTER TABLE [dbo].[menu_details]  WITH CHECK ADD  CONSTRAINT [FK_menu_details_branch_menus_menu_id] FOREIGN KEY([menu_id])
REFERENCES [dbo].[branch_menus] ([menu_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[menu_details] CHECK CONSTRAINT [FK_menu_details_branch_menus_menu_id]
GO
ALTER TABLE [dbo].[menu_details]  WITH CHECK ADD  CONSTRAINT [FK_menu_details_employees_updated_by] FOREIGN KEY([updated_by])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[menu_details] CHECK CONSTRAINT [FK_menu_details_employees_updated_by]
GO
ALTER TABLE [dbo].[menu_details]  WITH CHECK ADD  CONSTRAINT [FK_menu_details_product_variants_variant_id] FOREIGN KEY([variant_id])
REFERENCES [dbo].[product_variants] ([variant_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[menu_details] CHECK CONSTRAINT [FK_menu_details_product_variants_variant_id]
GO
ALTER TABLE [dbo].[order_items]  WITH CHECK ADD  CONSTRAINT [FK_order_items_orders_order_id] FOREIGN KEY([order_id])
REFERENCES [dbo].[orders] ([order_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[order_items] CHECK CONSTRAINT [FK_order_items_orders_order_id]
GO
ALTER TABLE [dbo].[order_items]  WITH CHECK ADD  CONSTRAINT [FK_order_items_product_variants_variant_id] FOREIGN KEY([variant_id])
REFERENCES [dbo].[product_variants] ([variant_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[order_items] CHECK CONSTRAINT [FK_order_items_product_variants_variant_id]
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD  CONSTRAINT [FK_orders_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
GO
ALTER TABLE [dbo].[orders] CHECK CONSTRAINT [FK_orders_branches_branch_id]
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD  CONSTRAINT [FK_orders_employees_cashier_id] FOREIGN KEY([cashier_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[orders] CHECK CONSTRAINT [FK_orders_employees_cashier_id]
GO
ALTER TABLE [dbo].[orders]  WITH CHECK ADD  CONSTRAINT [FK_orders_vouchers] FOREIGN KEY([voucher_id])
REFERENCES [dbo].[vouchers] ([voucher_id])
GO
ALTER TABLE [dbo].[orders] CHECK CONSTRAINT [FK_orders_vouchers]
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD  CONSTRAINT [FK_payments_branches] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
GO
ALTER TABLE [dbo].[payments] CHECK CONSTRAINT [FK_payments_branches]
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD  CONSTRAINT [FK_payments_employees] FOREIGN KEY([cashier_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[payments] CHECK CONSTRAINT [FK_payments_employees]
GO
ALTER TABLE [dbo].[payments]  WITH CHECK ADD  CONSTRAINT [FK_payments_orders] FOREIGN KEY([order_id])
REFERENCES [dbo].[orders] ([order_id])
GO
ALTER TABLE [dbo].[payments] CHECK CONSTRAINT [FK_payments_orders]
GO
ALTER TABLE [dbo].[product_variants]  WITH CHECK ADD  CONSTRAINT [FK_product_variants_master_products_product_id] FOREIGN KEY([product_id])
REFERENCES [dbo].[master_products] ([product_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[product_variants] CHECK CONSTRAINT [FK_product_variants_master_products_product_id]
GO
ALTER TABLE [dbo].[recipes]  WITH CHECK ADD  CONSTRAINT [FK_recipes_materials_material_id] FOREIGN KEY([material_id])
REFERENCES [dbo].[materials] ([material_id])
GO
ALTER TABLE [dbo].[recipes] CHECK CONSTRAINT [FK_recipes_materials_material_id]
GO
ALTER TABLE [dbo].[recipes]  WITH CHECK ADD  CONSTRAINT [FK_recipes_product_variants_variant_id] FOREIGN KEY([variant_id])
REFERENCES [dbo].[product_variants] ([variant_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[recipes] CHECK CONSTRAINT [FK_recipes_product_variants_variant_id]
GO
ALTER TABLE [dbo].[shift_change_requests]  WITH CHECK ADD  CONSTRAINT [FK_shift_change_requests_branch_managers_approved_branch_id_approved_manager_id] FOREIGN KEY([approved_branch_id], [approved_manager_id])
REFERENCES [dbo].[branch_managers] ([branch_id], [manager_id])
GO
ALTER TABLE [dbo].[shift_change_requests] CHECK CONSTRAINT [FK_shift_change_requests_branch_managers_approved_branch_id_approved_manager_id]
GO
ALTER TABLE [dbo].[shift_change_requests]  WITH CHECK ADD  CONSTRAINT [FK_shift_change_requests_employees_requesting_employee_id] FOREIGN KEY([requesting_employee_id])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[shift_change_requests] CHECK CONSTRAINT [FK_shift_change_requests_employees_requesting_employee_id]
GO
ALTER TABLE [dbo].[vouchers]  WITH CHECK ADD  CONSTRAINT [FK_vouchers_branches] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
GO
ALTER TABLE [dbo].[vouchers] CHECK CONSTRAINT [FK_vouchers_branches]
GO
ALTER TABLE [dbo].[vouchers]  WITH CHECK ADD  CONSTRAINT [FK_vouchers_employees] FOREIGN KEY([created_by])
REFERENCES [dbo].[employees] ([employee_id])
GO
ALTER TABLE [dbo].[vouchers] CHECK CONSTRAINT [FK_vouchers_employees]
GO
ALTER TABLE [dbo].[warehouse_receipt_items]  WITH CHECK ADD  CONSTRAINT [FK_warehouse_receipt_items_materials_material_id] FOREIGN KEY([material_id])
REFERENCES [dbo].[materials] ([material_id])
GO
ALTER TABLE [dbo].[warehouse_receipt_items] CHECK CONSTRAINT [FK_warehouse_receipt_items_materials_material_id]
GO
ALTER TABLE [dbo].[warehouse_receipt_items]  WITH CHECK ADD  CONSTRAINT [FK_warehouse_receipt_items_warehouse_receipts_receipt_id] FOREIGN KEY([receipt_id])
REFERENCES [dbo].[warehouse_receipts] ([receipt_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[warehouse_receipt_items] CHECK CONSTRAINT [FK_warehouse_receipt_items_warehouse_receipts_receipt_id]
GO
ALTER TABLE [dbo].[weekly_roster_grids]  WITH CHECK ADD  CONSTRAINT [FK_weekly_roster_grids_branches_branch_id] FOREIGN KEY([branch_id])
REFERENCES [dbo].[branches] ([branch_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[weekly_roster_grids] CHECK CONSTRAINT [FK_weekly_roster_grids_branches_branch_id]
GO
ALTER TABLE [dbo].[weekly_roster_grids]  WITH CHECK ADD  CONSTRAINT [FK_weekly_roster_grids_employees_employee_id] FOREIGN KEY([employee_id])
REFERENCES [dbo].[employees] ([employee_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[weekly_roster_grids] CHECK CONSTRAINT [FK_weekly_roster_grids_employees_employee_id]
GO
ALTER TABLE [dbo].[weekly_roster_grids]  WITH CHECK ADD  CONSTRAINT [FK_weekly_roster_grids_fixed_shifts_shift_id] FOREIGN KEY([shift_id])
REFERENCES [dbo].[fixed_shifts] ([shift_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[weekly_roster_grids] CHECK CONSTRAINT [FK_weekly_roster_grids_fixed_shifts_shift_id]
GO

/*******************************************************************************
   PART 4: RE-ENABLE & CHECK ALL CONSTRAINTS
*******************************************************************************/
EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all';
GO
PRINT 'Database restoration and seed completed successfully!';
GO

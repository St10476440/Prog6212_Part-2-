-- Fresh Part 3 database only. Does not drop or change existing tables.
USE [master];
GO
IF DB_ID(N'RaceDayPart3') IS NULL
    EXEC(N'CREATE DATABASE [RaceDayPart3]');
GO
USE [RaceDayPart3];
GO
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
        THROW 50001, 'RaceDayPart3 already contains tables. No changes made. Do not rerun this initial setup on an existing database.', 1;

    CREATE TABLE [dbo].[Venue] (
        [VenueID] nvarchar(450) NOT NULL,
        [VenueName] nvarchar(max) NOT NULL,
        [City] nvarchar(max) NOT NULL,
        [Province] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Venue] PRIMARY KEY ([VenueID])
    );

    CREATE TABLE [dbo].[Users] (
        [UserID] nvarchar(450) NOT NULL,
        [Password] nvarchar(max) NOT NULL,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NOT NULL,
        [DateOfBirth] date NULL,
        [EmergencyContact] nvarchar(max) NULL,
        [ProfilePictureUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserID])
    );

    CREATE TABLE [dbo].[Events] (
        [EventID] nvarchar(450) NOT NULL,
        [OrganiserID] nvarchar(450) NOT NULL,
        [VenueID] nvarchar(450) NOT NULL,
        [EventName] nvarchar(max) NOT NULL,
        [EventType] nvarchar(max) NOT NULL,
        [EventDate] date NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Location] nvarchar(max) NOT NULL,
        [DistanceKm] decimal(10,3) NOT NULL,
        [BannerImageUrl] nvarchar(max) NULL,
        CONSTRAINT [PK_Events] PRIMARY KEY ([EventID]),
        CONSTRAINT [FK_Events_Users_OrganiserID] FOREIGN KEY ([OrganiserID]) REFERENCES [dbo].[Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Events_Venue_VenueID] FOREIGN KEY ([VenueID]) REFERENCES [dbo].[Venue] ([VenueID]) ON DELETE NO ACTION
    );

    CREATE TABLE [dbo].[Categories] (
        [CategoryID] nvarchar(450) NOT NULL,
        [EventID] nvarchar(450) NOT NULL,
        [CategoryName] nvarchar(max) NOT NULL,
        [Distance] nvarchar(max) NOT NULL,
        [Country] nvarchar(max) NOT NULL,
        [MinimumAge] int NULL,
        [MaximumAge] int NULL,
        [EntryFee] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryID]),
        CONSTRAINT [FK_Categories_Events_EventID] FOREIGN KEY ([EventID]) REFERENCES [dbo].[Events] ([EventID]) ON DELETE NO ACTION
    );

    CREATE TABLE [dbo].[Enrolments] (
        [EnrolmentID] nvarchar(450) NOT NULL,
        [ParticipantID] nvarchar(450) NOT NULL,
        [CategoryID] nvarchar(450) NOT NULL,
        [RegistrationDate] date NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Enrolments] PRIMARY KEY ([EnrolmentID]),
        CONSTRAINT [FK_Enrolments_Users_ParticipantID] FOREIGN KEY ([ParticipantID]) REFERENCES [dbo].[Users] ([UserID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrolments_Categories_CategoryID] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Categories] ([CategoryID]) ON DELETE NO ACTION
    );

    CREATE TABLE [dbo].[Results] (
        [ResultsID] nvarchar(450) NOT NULL,
        [EnrolmentID] nvarchar(450) NOT NULL,
        [FinishTime] time(7) NOT NULL,
        [Position] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Results] PRIMARY KEY ([ResultsID]),
        CONSTRAINT [FK_Results_Enrolments_EnrolmentID] FOREIGN KEY ([EnrolmentID]) REFERENCES [dbo].[Enrolments] ([EnrolmentID]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_Events_OrganiserID] ON [dbo].[Events] ([OrganiserID]);
    CREATE INDEX [IX_Events_VenueID] ON [dbo].[Events] ([VenueID]);
    CREATE INDEX [IX_Categories_EventID] ON [dbo].[Categories] ([EventID]);
    CREATE INDEX [IX_Enrolments_ParticipantID] ON [dbo].[Enrolments] ([ParticipantID]);
    CREATE INDEX [IX_Enrolments_CategoryID] ON [dbo].[Enrolments] ([CategoryID]);
    CREATE INDEX [IX_Results_EnrolmentID] ON [dbo].[Results] ([EnrolmentID]);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

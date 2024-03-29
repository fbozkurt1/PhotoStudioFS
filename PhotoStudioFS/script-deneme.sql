info: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core 2.2.0-rtm-35687 initialized 'photostudioContext' using provider 'Pomelo.EntityFrameworkCore.MySql' with options: NoTracking 
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `AspNetRoles` (
    `Id` varchar(255) NOT NULL,
    `Name` varchar(256) NULL,
    `NormalizedName` varchar(256) NULL,
    `ConcurrencyStamp` longtext NULL,
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
);

CREATE TABLE `AspNetUsers` (
    `Id` varchar(255) NOT NULL,
    `UserName` varchar(256) NULL,
    `NormalizedUserName` varchar(256) NULL,
    `Email` varchar(256) NULL,
    `NormalizedEmail` varchar(256) NULL,
    `EmailConfirmed` bit NOT NULL,
    `PasswordHash` longtext NULL,
    `SecurityStamp` longtext NULL,
    `ConcurrencyStamp` longtext NULL,
    `PhoneNumber` longtext NULL,
    `PhoneNumberConfirmed` bit NOT NULL,
    `TwoFactorEnabled` bit NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` bit NOT NULL,
    `AccessFailedCount` int NOT NULL,
    `FullName` varchar(50) NOT NULL,
    `IsEnabled` bit NOT NULL,
    `IsFirstLogin` bit NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
);

CREATE TABLE `Contacts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(50) NOT NULL,
    `Email` varchar(50) NOT NULL,
    `Phone` varchar(15) NOT NULL,
    `Subject` varchar(200) NOT NULL,
    `Message` varchar(2000) NOT NULL,
    `IsRead` bit NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    CONSTRAINT `PK_Contacts` PRIMARY KEY (`Id`)
);

CREATE TABLE `Schedules` (
    `id` int NOT NULL AUTO_INCREMENT,
    `title` varchar(50) NULL,
    `start` datetime(6) NOT NULL,
    `end` datetime(6) NOT NULL,
    `photoShootType` varchar(20) NULL,
    `isEmpty` bit NOT NULL,
    `allDay` bit NOT NULL,
    CONSTRAINT `PK_Schedules` PRIMARY KEY (`id`)
);

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Appointments` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(50) NOT NULL,
    `Email` varchar(50) NOT NULL,
    `Phone` varchar(15) NOT NULL,
    `Type` varchar(20) NOT NULL,
    `Message` varchar(1000) NULL,
    `AppointmentDateStart` datetime(6) NOT NULL,
    `AppointmentDateEnd` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `IsApproved` smallint NOT NULL,
    `ScheduleId` int NOT NULL,
    `State` smallint NOT NULL,
    `StateUpdateDate` datetime(6) NOT NULL,
    `Price` decimal(9, 2) NOT NULL,
    `PricePaid` decimal(9, 2) NOT NULL,
    `CustomerId` varchar(255) NULL,
    CONSTRAINT `PK_Appointments` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Appointments_AspNetUsers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
);

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) NOT NULL,
    `ClaimType` longtext NULL,
    `ClaimValue` longtext NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(255) NOT NULL,
    `ProviderKey` varchar(255) NOT NULL,
    `ProviderDisplayName` longtext NULL,
    `UserId` varchar(255) NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(255) NOT NULL,
    `RoleId` varchar(255) NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(255) NOT NULL,
    `LoginProvider` varchar(255) NOT NULL,
    `Name` varchar(255) NOT NULL,
    `Value` longtext NULL,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
);

CREATE TABLE `Photos` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Path` varchar(1000) NULL,
    `ThumbnailPath` longtext NULL,
    `FileName` longtext NULL,
    `AppointmentId` int NOT NULL,
    `CustomerId` varchar(255) NULL,
    CONSTRAINT `PK_Photos` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Photos_Appointments_AppointmentId` FOREIGN KEY (`AppointmentId`) REFERENCES `Appointments` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Photos_AspNetUsers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE RESTRICT
);

CREATE INDEX `IX_Appointments_CustomerId` ON `Appointments` (`CustomerId`);

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_Photos_AppointmentId` ON `Photos` (`AppointmentId`);

CREATE INDEX `IX_Photos_CustomerId` ON `Photos` (`CustomerId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20200126201104_initial', '2.2.0-rtm-35687');



# Secure File Vault API

A secure and scalable RESTful API for personal file storage and management, built with .NET 9 and Entity Framework Core. This project enables users to sign up, log in using JWT-based authentication, and securely upload, organize, and manage their files and folders with support for Azure Blob Storage integration.

---

## Table of Contents

1. [Key Features](#key-features)  
2. [Tech Stack](#tech-stack)  
3. [Prerequisites](#prerequisites)  
4. [Setup & Installation](#setup--installation)  
   - [Clone the Repository](#clone-the-repository)  
   - [Configure `appsettings.json`](#configure-appsettingsjson)  
   - [Apply Migrations](#apply-migrations)  
   - [Run the API](#run-the-api)  
5. [Folder Structure](#folder-structure)  
6. [Usage](#usage)  
   - [Authentication Endpoints](#authentication-endpoints)  
   - [File & Folder Endpoints](#file--folder-endpoints)   

---

## Key Features

- **User Authentication & Authorization**  
  - JWT-based signup and login  
  - Password hashing with HMACSHA512  
  - Role-based access (optional)  

- **File Management**  
  - Upload, download, delete, and restore files  
  - Soft delete (trash) functionality with timestamps  
  - File metadata stored in SQL Server via EF Core  

- **Folder Hierarchy**  
  - Create, rename, delete, and nest folders  
  - Each folder tied to a specific user  

- **Secure Storage**  
  - File contents stored in Azure Blob Storage  
  - `BlobStorageName` saved in database for lookup  
  - Time-limited SAS URLs for secure downloads  

- **Data Validation & Constraints**  
  - Data annotations for model validation  
  - Fluent API for additional constraints (unique indexes, query filters)  

---

## Tech Stack

- **API**: .NET 9 (ASP.NET Core Web API)  
- **ORM**: Entity Framework Core (Code First)  
- **Database**: SQL Server (or Azure SQL Database)  
- **Storage**: Azure Blob Storage  
- **Authentication**: JWT (JSON Web Tokens)  
- **Language**: C#  
- **Tools**: Swagger (Swashbuckle), Visual Studio / VS Code  

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)  
- SQL Server instance (local or Azure)  
- Azure Storage Account (for Blob Storage)  
- Visual Studio 2022 / VS Code (or any preferred code editor)  
- (Optional) Postman or similar API client  

---

## Setup & Installation

### Clone the Repository

```bash
git clone https://github.com/Vikas-Devnet/SecureFileVaultAPI.git
cd SecureFileVaultAPI
